using System.Text.Json.Serialization;

namespace SPO.ColdStorage.Migration.Engine.Model
{
    public class SharePointFileInfo
    {
        /// <summary>
        /// Example: https://m365x352268.sharepoint.com/sites/MigrationHost
        /// </summary>
        public string SiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// Example: /sites/MigrationHost/Shared%20Documents/Contoso.pptx
        /// </summary>
        public string FileRelativePath { get; set; } = string.Empty;

        [JsonIgnore]
        public string FullUrl => SiteUrl + FileRelativePath;

        [JsonIgnore]
        public virtual bool IsValidInfo => !string.IsNullOrEmpty(FileRelativePath) && !string.IsNullOrEmpty(SiteUrl);
    }

    public class SharePointFileUpdateInfo : SharePointFileInfo
    {
        public DateTime LastModified { get; set; } = DateTime.MinValue;
    }

    public class SharePointFileInfoEventArgs : EventArgs
    {
        public SharePointFileInfoEventArgs()
        {
            this.SharePointFileInfo = new SharePointFileUpdateInfo();
        }
        public SharePointFileUpdateInfo SharePointFileInfo { get; set; }
    }
}
