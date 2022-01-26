using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPO.ColdStorage.Migration.Engine.Model;
using System;

namespace SPO.ColdStorage.Tests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void FileSearchModelTests()
        {
            // Normal SP path
            var searchObj1 = new FileSearchModel(new SharePointFileInfo
            { 
                ServerRelativeFilePath = "/sites/MigrationHost/Shared%20Documents/Blank%20Office%20PPT.pptx",
                SiteUrl = "https://m365x352268.sharepoint.com/sites/MigrationHost"
            });

            Assert.IsTrue(searchObj1.FoldersDeep == 3);


            // Normalish SP path
            var searchObj2 = new FileSearchModel(new SharePointFileInfo
            {
                ServerRelativeFilePath = "/sites/Blank%20Office%20PPT.pptx",
                SiteUrl = "https://m365x352268.sharepoint.com/sites/MigrationHost"
            });

            Assert.IsTrue(searchObj2.FoldersDeep == 1);



            // Invalid SP path
            var searchObj3 = new FileSearchModel(new SharePointFileInfo
            {
                ServerRelativeFilePath = "Blank%20Office%20PPT.pptx",
                SiteUrl = "https://m365x352268.sharepoint.com/sites/MigrationHost"
            });

            Assert.IsTrue(searchObj3.FoldersDeep == 0);
        }

        [TestMethod]
        public void SharePointFileInfoTests()
        {
            var emptyMsg1 = new SharePointFileInfo { };
            Assert.IsFalse(emptyMsg1.IsValidInfo);

            var halfEmptyMsg = new SharePointFileInfo { ServerRelativeFilePath = "/subweb1/whatever.txt" };
            Assert.IsFalse(halfEmptyMsg.IsValidInfo);

            // File path doesn't contain web
            var invalidMsg1 = new SharePointFileInfo
            { 
                ServerRelativeFilePath = "/whatever", 
                SiteUrl = "https://m365x352268.sharepoint.com", 
                WebUrl = "https://m365x352268.sharepoint.com/subweb1",
                LastModified = DateTime.Now
            };
            Assert.IsFalse(invalidMsg1.IsValidInfo);

            // Trailing slashes
            var invalidMsg2 = new SharePointFileInfo
            {
                ServerRelativeFilePath = "/whatever",
                SiteUrl = "https://m365x352268.sharepoint.com/",
                WebUrl = "https://m365x352268.sharepoint.com/subweb1/",
                LastModified = DateTime.Now
            };
            Assert.IsFalse(invalidMsg2.IsValidInfo);

            // Missing start slash on file path
            var invalidMsg3 = new SharePointFileInfo
            {
                ServerRelativeFilePath = "subweb1/whatever",
                SiteUrl = "https://m365x352268.sharepoint.com",
                WebUrl = "https://m365x352268.sharepoint.com/subweb1",
                LastModified = DateTime.Now
            };
            Assert.IsFalse(invalidMsg3.IsValidInfo);

            var validMsg1 = new SharePointFileInfo
            {
                ServerRelativeFilePath = "/subweb1/whatever",
                SiteUrl = "https://m365x352268.sharepoint.com",
                WebUrl = "https://m365x352268.sharepoint.com/subweb1",
                LastModified = DateTime.Now
            };
            Assert.IsTrue(validMsg1.IsValidInfo);

            Assert.IsTrue(validMsg1.FullUrl == "https://m365x352268.sharepoint.com/subweb1/whatever");
        }
    }
}
