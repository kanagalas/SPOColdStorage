using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Entities.Configuration;
using SPO.ColdStorage.Migration.Engine.Model;

namespace SPO.ColdStorage.Migration.Engine.Migration
{
    /// <summary>
    /// Downloads files from SharePoint to local file-system
    /// </summary>
    public class SharePointFileDownloader : BaseComponent
    {
        private ClientContext _context;
        public SharePointFileDownloader(ClientContext clientContext, Config config, DebugTracer debugTracer) :base(config, debugTracer)
        {
            _context = clientContext;
        }

        /// <summary>
        /// Download file & return temp file-name + size
        /// </summary>
        public async Task<(string, long)> DownloadFileToTempDir(SharePointFileLocationInfo sharePointFile) 
        {
            _context.Load(_context.Web);
            var filetoDownload = _context.Web.GetFileByServerRelativeUrl(sharePointFile.FileRelativePath);
            _context.Load(filetoDownload);
            await _context.ExecuteQueryAsync();

            // Write to temp file
            var tempFileName = GetTempFileNameAndCreateDir(sharePointFile);

            _tracer.TrackTrace($"Downloading SharePoint file '{sharePointFile.FullUrl}'...", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);

            var spStreamResult = filetoDownload.OpenBinaryStream();
            await _context.ExecuteQueryAsync();

            var fileSize = spStreamResult.Value.Length;
            using (var fs = spStreamResult.Value)
            {
                using (var fileStream = System.IO.File.Create(tempFileName))
                {
                    spStreamResult.Value.Seek(0, SeekOrigin.Begin);
                    spStreamResult.Value.CopyTo(fileStream);
                }
            }
            _tracer.TrackTrace($"Wrote {fileSize.ToString("N0")} bytes to '{tempFileName}'.", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);

            return (tempFileName, fileSize);
        }

        public static string GetTempFileNameAndCreateDir(SharePointFileLocationInfo sharePointFile)
        {
            var tempFileName = Path.GetTempPath().TrimEnd(@"\".ToCharArray()) + sharePointFile.FileRelativePath.Replace("/", @"\");
            var tempFileInfo = new FileInfo(tempFileName);
            Directory.CreateDirectory(tempFileInfo.DirectoryName!);

            return tempFileName;
        }
    }
}
