using System.Text.Json.Serialization;

namespace SPO.ColdStorage.Migration.Engine.Model
{
    /// <summary>
    /// SharePoint Online file metadata
    /// </summary>
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
        public string ServerRelativeFilePath { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public DateTime LastModified { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Calculated.
        /// </summary>
        [JsonIgnore]
        public bool IsValidInfo => !string.IsNullOrEmpty(ServerRelativeFilePath) && 
            !string.IsNullOrEmpty(SiteUrl) && 
            !string.IsNullOrEmpty(WebUrl) && 
            this.LastModified > DateTime.MinValue && 
            this.WebUrl.StartsWith(this.SiteUrl) &&
            this.FullUrl.StartsWith(this.WebUrl);

        /// <summary>
        /// Calculated. Web + file URL, minus overlap, if both are valid.
        /// </summary>
        [JsonIgnore]
        public string FullUrl
        {
            get 
            {
                // Strip out relative web part of file URL
                const string DOMAIN = "sharepoint.com";
                var domainStart = WebUrl.IndexOf(DOMAIN, StringComparison.CurrentCultureIgnoreCase);
                if (domainStart > -1)
                {
                    var webMinusServer = WebUrl.Substring(domainStart + DOMAIN.Length, (WebUrl.Length - domainStart) - DOMAIN.Length);

                    if (ServerRelativeFilePath.StartsWith(webMinusServer))
                    {
                        var filePathWithoutWeb = ServerRelativeFilePath.Substring(webMinusServer.Length, ServerRelativeFilePath.Length - webMinusServer.Length);
                        return WebUrl + filePathWithoutWeb;
                    }
                    else
                    {
                        return ServerRelativeFilePath;
                    }
                }
                else
                {
                    return ServerRelativeFilePath;
                }
            }
        }
    }
}
