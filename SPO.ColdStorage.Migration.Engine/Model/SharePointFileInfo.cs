using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine.Model
{
    public class SharePointFileInfo
    {
        public string Url { get; set; } = string.Empty;
        public DateTime LastModified { get; set; } = DateTime.MinValue;
    }

    public class SharePointFileInfoEventArgs : EventArgs
    {
        public SharePointFileInfoEventArgs()
        {
            this.SharePointFileInfo = new SharePointFileInfo();
        }
        public SharePointFileInfo SharePointFileInfo { get; set; }
    }
}
