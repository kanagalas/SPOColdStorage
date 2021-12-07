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
        /// Example: https://m365x352268.sharepoint.com/sites/MigrationHost/subsite
        /// </summary>
        public string WebUrl { get; set; } = string.Empty;

        /// <summary>
        /// Example: /sites/MigrationHost/Shared%20Documents/Contoso.pptx
        /// </summary>
        public string FileRelativePath { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public DateTime LastModified { get; set; } = DateTime.MinValue;
        
        [JsonIgnore]
        public bool IsValidInfo => !string.IsNullOrEmpty(FileRelativePath) && !string.IsNullOrEmpty(SiteUrl) && !string.IsNullOrEmpty(WebUrl) && this.LastModified > DateTime.MinValue;

        [JsonIgnore]
        public string FullUrl => SiteUrl + FileRelativePath;

    }
}
