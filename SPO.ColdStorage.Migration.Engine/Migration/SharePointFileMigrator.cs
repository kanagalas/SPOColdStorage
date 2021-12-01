using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Migration.Engine.Model;

namespace SPO.ColdStorage.Migration.Engine.Migration
{
    public class SharePointFileMigrator : BaseComponent
    {
        private ServiceBusClient _sbClient;
        private ServiceBusSender _sbSender;
        public SharePointFileMigrator(Config config) : base(config)
        {
            _sbClient = new ServiceBusClient(_config.ServiceBusConnectionString);
            _sbSender = _sbClient.CreateSender(_config.ServiceBusQueueName);
        }

        public async Task MigrateSharePointFileIfNeeded(SharePointFileUpdateInfo sharePointFileInfo, BlobContainerClient containerClient)
        {
            bool needsMigrating = SharePointFileNeedsMigrating(sharePointFileInfo, containerClient);
            if (needsMigrating)
            {
                // Send msg to migrate file
                var newFileMessage = new SharePointFileInfo
                {
                    FileRelativePath = sharePointFileInfo.FileRelativePath,
                    SiteUrl = sharePointFileInfo.SiteUrl
                };
                var sbMsg = new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(newFileMessage));
                await _sbSender.SendMessageAsync(sbMsg);
                _tracer.TrackTrace($"+'{sharePointFileInfo.FullUrl}'...");
            }
            else
            {
                _tracer.TrackTrace($"-'{sharePointFileInfo.FullUrl}'...");
            }
        }

        public bool SharePointFileNeedsMigrating(SharePointFileUpdateInfo sharePointFileInfo, BlobContainerClient containerClient)
        {
            // Check existing blobs
            var blobs = containerClient.GetBlobs(BlobTraits.Metadata, BlobStates.None, sharePointFileInfo.FileRelativePath);

            var count = blobs?.Count() ?? 0;
            return count == 0;
        }

        public async Task MigrateFromSharePointToBlobStorage(SharePointFileInfo msg, ClientContext ctx)
        {
            // Download from SP to local
            var downloader = new SharePointFileDownloader(ctx, _config);
            var tempFileName = await downloader.DownloadFileToTempDir(msg);

            // Index file properties
            var searchIndexer = new SharePointFileSearchProcessor(_config);
            await searchIndexer.ProcessFileContent(msg);

            // Upload local file to az blob
            var blobUploader = new BlobStorageUploader(_config);
            await blobUploader.UploadFileToAzureBlob(tempFileName, msg);

            // Clean-up
            System.IO.File.Delete(tempFileName);
        }
    }
}
