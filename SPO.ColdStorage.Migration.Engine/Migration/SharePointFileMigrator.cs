using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Migration.Engine.Model;

namespace SPO.ColdStorage.Migration.Engine.Migration
{
    public class SharePointFileMigrator : BaseComponent
    {
        private ServiceBusClient _sbClient;
        private ServiceBusSender _sbSender;
        public SharePointFileMigrator(Config config, DebugTracer debugTracer) : base(config, debugTracer)
        {
            _sbClient = new ServiceBusClient(_config.ServiceBusConnectionString);
            _sbSender = _sbClient.CreateSender(_config.ServiceBusQueueName);
        }

        public async Task QueueSharePointFileMigrationIfNeeded(SharePointFileUpdateInfo sharePointFileInfo, BlobContainerClient containerClient)
        {
            bool needsMigrating = await SharePointFileNeedsMigrating(sharePointFileInfo, containerClient);
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
        }

        public async Task<bool> SharePointFileNeedsMigrating(SharePointFileUpdateInfo sharePointFileInfo, BlobContainerClient containerClient)
        {
            // Check existing blobs
            var fileRef = containerClient.GetBlobClient(sharePointFileInfo.FileRelativePath);
            var fileExists = await fileRef.ExistsAsync();

            return !fileExists;
        }

        public async Task<long> MigrateFromSharePointToBlobStorage(SharePointFileInfo msg, ClientContext ctx)
        {
            // Download from SP to local
            var downloader = new SharePointFileDownloader(ctx, _config, _tracer);
            var tempFileNameAndSize = await downloader.DownloadFileToTempDir(msg);

            // Index file properties
            var searchIndexer = new SharePointFileSearchProcessor(_config, _tracer);
            await searchIndexer.ProcessFileContent(msg);

            // Upload local file to az blob
            var blobUploader = new BlobStorageUploader(_config, _tracer);
            await blobUploader.UploadFileToAzureBlob(tempFileNameAndSize.Item1, msg);

            // Clean-up
            try
            {
                System.IO.File.Delete(tempFileNameAndSize.Item1);
            }
            catch (IOException ex)
            {
                _tracer.TrackTrace($"Got errror {ex.Message} cleaning temp file '{tempFileNameAndSize.Item1}'. Ignoring.", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning);
            }

            // Return file-size
            return tempFileNameAndSize.Item2;
        }
    }
}
