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

        [TestMethod]
        public async Task FileMigratorTests()
        {
            var testMsg = new SharePointFileInfo 
            { 
                SiteUrl = "https://m365x352268.sharepoint.com/sites/MigrationHost", 
                FileRelativePath = "/sites/MigrationHost/Shared%20Documents/Blank%20Office%20PPT.pptx"
            };
            var ctx = await AuthUtils.GetClientContext(_config!, testMsg.SiteUrl);

            var m = new FileMigrator(ctx, _config!);
            await m.MigrateSharePointFileToBlobStorage(testMsg);
        }

        [TestMethod]
        public async Task FileContentProcessorTests()
        {
            var testMsg = new SharePointFileInfo
            {
                SiteUrl = "https://m365x352268.sharepoint.com/sites/MigrationHost",
                FileRelativePath = "/sites/MigrationHost/Shared%20Documents/Blank%20Office%20PPT.pptx"
            };

            var m = new SharePointFileProcessor(_config!);
            await m.ProcessFileContent(testMsg);
        }
    }
}
