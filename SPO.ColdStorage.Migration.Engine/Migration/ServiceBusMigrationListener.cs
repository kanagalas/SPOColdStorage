using Azure.Messaging.ServiceBus;
using SPO.ColdStorage.Entities.Configuration;
using SPO.ColdStorage.Migration.Engine.Migration;
using SPO.ColdStorage.Migration.Engine.Model;
using System.Collections.Concurrent;

namespace SPO.ColdStorage.Migration.Engine
{
    /// <summary>
    /// Listens for new service bus messages for files to migrate to az blob
    /// </summary>
    public class ServiceBusMigrationListener : BaseComponent, IDisposable
    {
        private ServiceBusClient _sbClient;
        private ServiceBusProcessor _processor;
        private SharePointFileMigrator _sharePointFileMigrator;
        private ConcurrentBag<string> cb = new();

        public ServiceBusMigrationListener(Config config, DebugTracer debugTracer) : base(config, debugTracer)
        {
            _sbClient = new ServiceBusClient(_config.ConnectionStrings.ServiceBus);
            _processor = _sbClient.CreateProcessor(_config.ServiceBusQueueName, new ServiceBusProcessorOptions());
            _sharePointFileMigrator = new SharePointFileMigrator(config, debugTracer);
        }

        public async Task ListenForFilesToMigrate()
        {
            try
            {
                // add handler to process messages
                _processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                _processor.ProcessErrorAsync += ErrorHandler;

                var sbConnectionProps = ServiceBusConnectionStringProperties.Parse(_config.ConnectionStrings.ServiceBus);
                _tracer.TrackTrace($"Listening on service-bus '{sbConnectionProps.Endpoint}' for new files to migrate.");

                // start processing 
                await _processor.StartProcessingAsync();

                while (true)
                {
                    await Task.Delay(1000);
                }
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await _processor.DisposeAsync();
                await _sbClient.DisposeAsync();
            }
        }

        // Handle received SB messages
        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            var msg = System.Text.Json.JsonSerializer.Deserialize<SharePointFileVersionInfo>(body);
            if (msg != null && msg.IsValidInfo)
            {
                _tracer.TrackTrace($"Started migration for: {msg.FileRelativePath}");

                // Fire & forget file migration on background thread 
                _ = Task.Run(() => StartFileMigrationAsync(msg));

                // Complete the message. messages is deleted from the queue. 
                await args.CompleteMessageAsync(args.Message);
            }
            else
            {
                _tracer.TrackTrace($"Received unrecognised message: '{body}'. Sending to dead-letter queue.", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                await args.DeadLetterMessageAsync(args.Message);
            }
        }

        // Handle any errors when receiving SB messages
        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _tracer.TrackTrace(args.Exception.Message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
            _tracer.TrackException(args.Exception);
            return Task.CompletedTask;
        }

        private async Task StartFileMigrationAsync(SharePointFileVersionInfo sharePointFileToMigrate)
        {
            string thisFileRef = sharePointFileToMigrate.FullUrl;
            if (cb.Contains(thisFileRef))
            {
                _tracer.TrackTrace($"Already currently importing file '{sharePointFileToMigrate.FullUrl}'. Won't do it twice this session.", 
                    Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning);
                return;
            }

            // Find/create SP context
            var ctx = await AuthUtils.GetClientContext(_config, sharePointFileToMigrate.SiteUrl);

            // Begin migration on common class
            cb.Add(sharePointFileToMigrate.FileRelativePath);
            long migratedFileSize = 0;
            try
            {
                migratedFileSize = await _sharePointFileMigrator.MigrateFromSharePointToBlobStorage(sharePointFileToMigrate, ctx);
            }
            catch (Exception ex)
            {
                _tracer.TrackException(ex);
                _tracer.TrackTrace($"ERROR: Got fatal error '{ex.Message}' importing file '{sharePointFileToMigrate.FullUrl}'", 
                    Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
            }
            finally 
            {
                // Import done/failed - remove from list of current imports
                if (!cb.TryTake(out thisFileRef!))
                {
                    _tracer.TrackTrace($"Error removing file '{sharePointFileToMigrate.FullUrl}' from list of concurrent operations. Not sure what to do.",
                        Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning);
                }
                else
                {
                    _tracer.TrackTrace($"File '{sharePointFileToMigrate.FullUrl}' ({migratedFileSize.ToString("N0")} bytes) migrated succesfully.");
                }
            }
        }

        public void Dispose()
        {
            _sharePointFileMigrator.Dispose();
        }
    }
}
