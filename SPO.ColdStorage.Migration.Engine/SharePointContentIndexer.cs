using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Entities.Configuration;
using SPO.ColdStorage.Migration.Engine.Migration;
using SPO.ColdStorage.Migration.Engine.Model;

namespace SPO.ColdStorage.Migration.Engine
{
    /// <summary>
    /// Finds files to migrate in a SharePoint site-collection
    /// </summary>
    public class SharePointContentIndexer : BaseComponent
    {
        private BlobServiceClient _blobServiceClient;
        private BlobContainerClient? _containerClient;
        private SharePointFileMigrator _sharePointFileMigrator;

        public SharePointContentIndexer(Config config, DebugTracer debugTracer) : base(config, debugTracer)
        {
            var sbConnectionProps = ServiceBusConnectionStringProperties.Parse(_config.ConnectionStrings.ServiceBus);
            _tracer.TrackTrace($"Sending new SharePoint files to migrate to service-bus '{sbConnectionProps.Endpoint}'.");


            // Create a BlobServiceClient object which will be used to create a container client
            _blobServiceClient = new BlobServiceClient(_config.ConnectionStrings.Storage);
            _sharePointFileMigrator = new SharePointFileMigrator(config, _tracer);
        }

        public async Task StartMigrateAllSites()
        {
            // Create the container and return a container client object
            this._containerClient = _blobServiceClient.GetBlobContainerClient(_config.BlobContainerName);

            // Create container with no access to public
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            using (var db = new SPOColdStorageDbContext(this._config))
            {
                var sitesToMigrate = await db.TargetSharePointSites.ToListAsync();
                foreach (var s in sitesToMigrate)
                {
                    await StartSiteMigration(s.RootURL);
                }
            }
        }

        async Task StartSiteMigration(string siteUrl)
        {
            var ctx = await AuthUtils.GetClientContext(_config, siteUrl);

            _tracer.TrackTrace($"Scanning site-collection '{siteUrl}'...");

            var crawler = new SiteListsAndLibrariesCrawler(ctx, _tracer, Crawler_SharePointFileFound);
            await crawler.CrawlContextRootWebAndSubwebs();
        }

        /// <summary>
        /// Crawler found a relevant file
        /// </summary>
        private async Task Crawler_SharePointFileFound(SharePointFileInfo foundFileInfo)
        {
            await _sharePointFileMigrator.QueueSharePointFileMigrationIfNeeded(foundFileInfo, _containerClient!);
        }
    }
}
