using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine.Model
{
    public class SharePointFileInfo
    {
        public string SiteUrl { get; set; } = string.Empty;
        public string FileRelativePath { get; set; } = string.Empty;

        [JsonIgnore]
        public string FullUrl => SiteUrl + FileRelativePath;

        [JsonIgnore]
        public virtual bool IsValid => !string.IsNullOrEmpty(FileRelativePath) && !string.IsNullOrEmpty(SiteUrl);
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
