using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Migration.Engine.Model;
using System.IO;

namespace SPO.ColdStorage.Migration.Engine.Migration
{
    public class SharePointFileDownloader : BaseComponent
    {
        private ClientContext _context;
        public SharePointFileDownloader(ClientContext clientContext, Config config) :base(config)
        {
            _context = clientContext;
        }

        public async Task<string> DownloadFileToTempDir(SharePointFileInfo sharePointFile) 
        {
            _context.Load(_context.Web);
            var filetoDownload = _context.Web.GetFileByServerRelativeUrl(sharePointFile.FileRelativePath);
            _context.Load(filetoDownload);
            await _context.ExecuteQueryAsync();

            // Write to temp file
            var tempFileName = GetTempFileNameAndCreateDir(sharePointFile);

            var spStreamResult = filetoDownload.OpenBinaryStream();
            await _context.ExecuteQueryAsync();


            using (var fs = spStreamResult.Value)
            {
                using (var fileStream = System.IO.File.Create(tempFileName))
                {
                    spStreamResult.Value.Seek(0, SeekOrigin.Begin);
                    spStreamResult.Value.CopyTo(fileStream);
                }
            }

            return tempFileName;
        }

        public static string GetTempFileNameAndCreateDir(SharePointFileInfo sharePointFile)
        {
            var tempFileName = Path.GetTempPath().TrimEnd(@"\".ToCharArray()) + sharePointFile.FileRelativePath.Replace("/", @"\");
            var tempFileInfo = new FileInfo(tempFileName);
            Directory.CreateDirectory(tempFileInfo.DirectoryName!);

            return tempFileName;
        }
    }
}
