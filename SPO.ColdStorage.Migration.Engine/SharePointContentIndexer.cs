using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Migration.Engine.Model;

namespace SPO.ColdStorage.Migration.Engine
{
    public class SharePointContentIndexer : BaseComponent
    {
        private ServiceBusClient _sbClient;
        private ServiceBusSender _sbSender;
        private BlobServiceClient _blobServiceClient;
        private BlobContainerClient? _containerClient;

        public SharePointContentIndexer(Config config) :base(config)
        {

            _sbClient = new ServiceBusClient(_config.ServiceBusConnectionString);
            _sbSender = _sbClient.CreateSender(_config.ServiceBusQueueName);

            // Create a BlobServiceClient object which will be used to create a container client
            _blobServiceClient = new BlobServiceClient(_config.StorageConnectionString);
        }

        public async Task StartMigrateNeededFiles()
        {
            // Create the container and return a container client object
            this._containerClient = _blobServiceClient.GetBlobContainerClient(_config.BlobContainerName);

            await _containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);

            using (var db = new ColdStorageDbContext(this._config.SQLConnectionString))
            {
                var migrations = await db.Migrations.Include(m => m.TargetSites).ToListAsync();
                foreach (var m in migrations)
                {
                    if (!m.Started.HasValue)
                    {
                        await StartMigration(m, db);
                    }
                }
            }
        }

        async Task StartMigration(Entities.DBEntities.SharePointMigration m, ColdStorageDbContext db)
        {
            foreach (var siteId in m.TargetSites)
            {
                await StartMigration(siteId.RootURL, db);
            }
        }
        async Task StartMigration(string siteUrl, ColdStorageDbContext db)
        {
            var ctx = await AuthUtils.GetClientContext(_config, siteUrl);

            _tracer.TrackTrace($"Migrating site '{siteUrl}'...");

            var crawler = new SiteCrawler(ctx, _tracer);
            crawler.SharePointFileFound += Crawler_SharePointFileFound;
            await crawler.Start();
        }

        private async void Crawler_SharePointFileFound(object? sender, SharePointFileInfoEventArgs e)
        {
            bool needsMigrating = FileNeedsMigrating(e.SharePointFileInfo);
            if (needsMigrating)
            {
                // Send msg to migrate file
                var newFileMessage = new SharePointFileInfo 
                { 
                    FileRelativePath = e.SharePointFileInfo.FileRelativePath,
                    SiteUrl = e.SharePointFileInfo.SiteUrl
                };
                var sbMsg = new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(newFileMessage));
                await _sbSender.SendMessageAsync(sbMsg);
                _tracer.TrackTrace($"+migrate file '{e.SharePointFileInfo.FullUrl}'...");
            }
            else
            {
                _tracer.TrackTrace($"Ignoring file '{e.SharePointFileInfo.FullUrl}'...");
            }
        }

        private bool FileNeedsMigrating(SharePointFileUpdateInfo sharePointFileInfo)
        {
            // Check existing blobs
            var blobs = _containerClient?.GetBlobs(BlobTraits.Metadata, BlobStates.None, sharePointFileInfo.FileRelativePath);

            var count = blobs?.Count() ?? 0;
            return count == 0;
        }
    }
}
