using Azure.Messaging.ServiceBus;
using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Migration.Engine.Migration;
using SPO.ColdStorage.Migration.Engine.Model;

namespace SPO.ColdStorage.Migration.Engine
{
    public class ServiceBusMigrationListener : BaseComponent
    {
        private ServiceBusClient _sbClient;
        private ServiceBusProcessor _processor;
        private bool _run = true;
        private Dictionary<string, ClientContext> _siteContexts = new();

        public ServiceBusMigrationListener(Config config) : base(config)
        {
            _sbClient = new ServiceBusClient(_config.ServiceBusConnectionString);
            _processor = _sbClient.CreateProcessor(_config.ServiceBusQueueName, new ServiceBusProcessorOptions());
            _run = true;
        }

        public async Task ListenForFilesToMigrate()
        {
            try
            {
                // add handler to process messages
                _processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                _processor.ProcessErrorAsync += ErrorHandler;

                var sbConnectionProps = ServiceBusConnectionStringProperties.Parse(_config.ServiceBusConnectionString);
                _tracer.TrackTrace($"Listening on service-bus '{sbConnectionProps.Endpoint}' for new files to migrate.");

                // start processing 
                await _processor.StartProcessingAsync();

                while (_run)
                {
                    await Task.Delay(1000);
                }

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                await _processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await _processor.DisposeAsync();
                await _sbClient.DisposeAsync();
            }
        }
        public void Stop()
        {
            _run = false;
        }

        // Handle received messages
        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            var msg = System.Text.Json.JsonSerializer.Deserialize<SharePointFileInfo>(body);
            if (msg != null && msg.IsValid)
            {
                _tracer.TrackTrace($"Received: {msg.FileRelativePath}");
                await StartMigration(msg);

                // complete the message. messages is deleted from the queue. 
                await args.CompleteMessageAsync(args.Message);
            }
            else
            {
                _tracer.TrackTrace($"Received unrecognised message: '{body}'. Sending to dead-letter queue.", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                await args.DeadLetterMessageAsync(args.Message);
            }

        }
        // Handle any errors when receiving SB messages
        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task StartMigration(SharePointFileInfo msg)
        {
            // Find/create SP context
            ClientContext ctx;
            if (!_siteContexts.ContainsKey(msg.SiteUrl))
            {
                _siteContexts.Add(msg.SiteUrl, await AuthUtils.GetClientContext(_config, msg.SiteUrl));
            }
            ctx = _siteContexts[msg.SiteUrl];

            // Download from SP and copy to blob
            var downloader = new SharePointFileDownloader(ctx, _config);
            var tempFileName = await downloader.DownloadFileToTempDir(msg);

            var searchIndexer = new SharePointFileSearchProcessor(_config);
            await searchIndexer.ProcessFileContent(msg);

            var blobUploader = new BlobStorageUploader(_config);
            await blobUploader.UploadFileToAzureBlob(tempFileName, msg);

            System.IO.File.Delete(tempFileName);
        }

    }
}
