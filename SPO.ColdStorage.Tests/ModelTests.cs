using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPO.ColdStorage.Migration.Engine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Tests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void FileSearchModelTests()
        {
            // Normal SP path
            var searchObj1 = new FileSearchModel(new SharePointFileLocationInfo 
            { 
                FileRelativePath = "/sites/MigrationHost/Shared%20Documents/Blank%20Office%20PPT.pptx",
                SiteUrl = "https://m365x352268.sharepoint.com/sites/MigrationHost"
            });

            Assert.IsTrue(searchObj1.FoldersDeep == 3);


            // Normalish SP path
            var searchObj2 = new FileSearchModel(new SharePointFileLocationInfo
            {
                FileRelativePath = "/sites/Blank%20Office%20PPT.pptx",
                SiteUrl = "https://m365x352268.sharepoint.com/sites/MigrationHost"
            });

            Assert.IsTrue(searchObj2.FoldersDeep == 1);



            // Invalid SP path
            var searchObj3 = new FileSearchModel(new SharePointFileLocationInfo
            {
                FileRelativePath = "Blank%20Office%20PPT.pptx",
                SiteUrl = "https://m365x352268.sharepoint.com/sites/MigrationHost"
            });

            Assert.IsTrue(searchObj3.FoldersDeep == 0);
        }

        [TestMethod]
        public void SharePointFileInfoTests()
        {
            var emptyMsg1 = new SharePointFileLocationInfo { };
            Assert.IsFalse(emptyMsg1.IsValidInfo);

            var halfEmptyMsg = new SharePointFileLocationInfo { FileRelativePath = "/whatever" };
            Assert.IsFalse(halfEmptyMsg.IsValidInfo);


            var legitMsg = new SharePointFileLocationInfo { FileRelativePath = "/whatever", SiteUrl = "https://m365x352268.sharepoint.com" };
            Assert.IsTrue(legitMsg.IsValidInfo);
        }
    }
}
