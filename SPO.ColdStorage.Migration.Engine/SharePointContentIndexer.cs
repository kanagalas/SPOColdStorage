using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Entities;
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

        public SharePointContentIndexer(Config config) :base(config)
        {
            var sbConnectionProps = ServiceBusConnectionStringProperties.Parse(_config.ServiceBusConnectionString);
            _tracer.TrackTrace($"Sending new SharePoint files to migrate to service-bus '{sbConnectionProps.Endpoint}'.");


            // Create a BlobServiceClient object which will be used to create a container client
            _blobServiceClient = new BlobServiceClient(_config.StorageConnectionString);
            _sharePointFileMigrator = new SharePointFileMigrator(config);
        }

        public async Task StartMigrateAllSites()
        {
            // Create the container and return a container client object
            this._containerClient = _blobServiceClient.GetBlobContainerClient(_config.BlobContainerName);

            // Create container with no access to public
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            using (var db = new ColdStorageDbContext(this._config.SQLConnectionString))
            {
                var migrations = await db.Migrations.Include(m => m.TargetSites).ToListAsync();
                foreach (var m in migrations)
                {
                    if (!m.Started.HasValue)
                    {
                        await StartMigration(m);
                    }
                }
            }
        }

        async Task StartMigration(Entities.DBEntities.SharePointMigration m)
        {
            foreach (var siteId in m.TargetSites)
            {
                await StartSiteMigration(siteId.RootURL);
            }
        }
        async Task StartSiteMigration(string siteUrl)
        {
            var ctx = await AuthUtils.GetClientContext(_config, siteUrl);

            _tracer.TrackTrace($"Migrating site '{siteUrl}'...");

            var crawler = new SiteListsAndLibrariesCrawler(ctx, _tracer);
            crawler.SharePointFileFound += Crawler_SharePointFileFound;
            await crawler.CrawlContextWeb();
        }

        /// <summary>
        /// Crawler found a relevant file
        /// </summary>
        private async void Crawler_SharePointFileFound(object? sender, SharePointFileInfoEventArgs e)
        {
            await _sharePointFileMigrator.MigrateSharePointFileIfNeeded(e.SharePointFileInfo, _containerClient!);
        }
    }
}
