using Azure.Search.Documents.Indexes;
using SPO.ColdStorage.Migration.Engine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine.Model
{
    public partial class FileSearchModel
    {
        public FileSearchModel()
        {
        }
        public FileSearchModel(SharePointFileInfo sharePointFileInfo)
        {

            var fileNameArray = sharePointFileInfo.FileRelativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            this.FileTitle = System.Net.WebUtility.UrlDecode(fileNameArray.LastOrDefault()) ?? String.Empty;
            this.Dir = GetDir(fileNameArray);

            this.FileId = StringUtils.Base64Encode(sharePointFileInfo.FileRelativePath);
        }


        public int FoldersDeep 
        { 
            get 
            {
                return GetSubFolderCount(this.Dir);
            } 
        }


        [SimpleField(IsKey = true, IsFilterable = true)]
        public string FileId { get; set; } = string.Empty;

        [SimpleField(IsFilterable = true)]
        public string FileTitle { get; set; } = string.Empty;

        [SimpleField(IsFilterable = true)]
        public string FilePath => $"{GetFolderName(Dir)} > {FileTitle}";

        [SearchableField]
        private FileDir? Dir { get; set; }

        #region Recursion

        FileDir? GetDir(IEnumerable<string> paths)
        {
            if (paths.Count() == 1)
            {
                return null;        // No folder; just a file-name
            }
            var dir = paths.First();
            var d = new FileDir() { Name = System.Net.WebUtility.UrlDecode(dir) };
            if (paths.Count() > 2)      // Ignore file-name
            {
                var remainingPaths = paths.Skip(1);
                d.Child = GetDir(remainingPaths);
            }
            return d;
        }
        int GetSubFolderCount(FileDir? dir)
        {
            if (dir is null)
            {
                return 0;
            }

            if (dir.Child != null)
            {
                return GetSubFolderCount(dir.Child) + 1;
            }
            else
                return 1;
        }
        string GetFolderName(FileDir? dir)
        {
            if (dir is null)
            {
                return string.Empty;
            }

            if (dir.Child != null)
            {
                return $"{dir.Name} > {GetFolderName(dir.Child)}";
            }
            else
                return dir.Name;
        }
        #endregion
    }

    public partial class FileDir
    {
        public string Name { get; set; } = string.Empty;


        public FileDir? Child { get; set; }
    }
}
