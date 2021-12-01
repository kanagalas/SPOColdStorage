using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Migration.Engine;
using SPO.ColdStorage.Migration.Engine.Migration;
using SPO.ColdStorage.Migration.Engine.Model;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Tests
{
    [TestClass]
    public class MigrationTests
    {
        #region Plumbing

        private Config? _config;

        [TestInitialize]
        public void Init()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly())
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", true);


            var config = builder.Build();
            _config = new Config(config);
        }
        #endregion

        [TestMethod]
        public async Task FileMigratorTests()
        {
            var testMsg = new SharePointFileInfo 
            { 
                SiteUrl = "https://m365x352268.sharepoint.com/sites/MigrationHost", 
                FileRelativePath = "/sites/MigrationHost/Shared%20Documents/Blank%20Office%20PPT.pptx"
            };
            var ctx = await AuthUtils.GetClientContext(_config!, testMsg.SiteUrl);

            var m = new SharePointFileDownloader(ctx, _config!);
            await m.DownloadFileToTempDir(testMsg);
        }

        [TestMethod]
        public async Task FileContentProcessorTests()
        {
            var testMsg = new SharePointFileInfo
            {
                SiteUrl = "https://m365x352268.sharepoint.com/sites/MigrationHost",
                FileRelativePath = "/sites/MigrationHost/Shared%20Documents/Blank%20Office%20PPT.pptx"
            };

            var m = new SharePointFileSearchProcessor(_config!);
            await m.ProcessFileContent(testMsg);
        }

        [TestMethod]
        public async Task FileUploadTests()
        {
            var testMsg = new SharePointFileInfo
            {
                SiteUrl = "https://m365x352268.sharepoint.com/sites/MigrationHost",
                FileRelativePath = "/sites/MigrationHost/Unit tests/textfile.txt"
            };
            const string FILE_CONTENTS = "En un lugar de la Mancha, de cuyo nombre no quiero acordarme, no ha mucho tiempo que vivía un hidalgo de los de lanza en astillero, adarga antigua, rocín flaco y galgo corredor";

            // Write a fake file
            string tempFileName = SharePointFileDownloader.GetTempFileNameAndCreateDir(testMsg);
            System.IO.File.WriteAllText(tempFileName, FILE_CONTENTS);

            var m = new BlobUploader(_config!);
            await m.UploadFileToAzureBlob(tempFileName, testMsg);
        }
    }
}
