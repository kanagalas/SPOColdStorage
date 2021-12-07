using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Entities.Configuration;
using SPO.ColdStorage.Entities.DBEntities;
using SPO.ColdStorage.Migration.Engine.Model;
using System;

namespace SPO.ColdStorage.Migration.Engine.Migration
{
    /// <summary>
    /// The top-level file migration logic for both indexer and migrator.
    /// </summary>
    public class SharePointFileMigrator : BaseComponent, IDisposable
    {
        private ServiceBusClient _sbClient;
        private ServiceBusSender _sbSender;
        private SPOColdStorageDbContext _db;
        public SharePointFileMigrator(Config config, DebugTracer debugTracer) : base(config, debugTracer)
        {
            _sbClient = new ServiceBusClient(_config.ConnectionStrings.ServiceBus);
            _sbSender = _sbClient.CreateSender(_config.ServiceBusQueueName);
            _db = new SPOColdStorageDbContext(_config);
        }

        /// <summary>
        /// Queue file for migrator to pick-up & migrate
        /// </summary>
        public async Task QueueSharePointFileMigrationIfNeeded(SharePointFileInfo sharePointFileInfo, BlobContainerClient containerClient)
        {
            bool needsMigrating = await DoesSharePointFileNeedMigrating(sharePointFileInfo, containerClient);
            if (needsMigrating)
            {
                // Send msg to migrate file
                var sbMsg = new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(sharePointFileInfo));
                await _sbSender.SendMessageAsync(sbMsg);
                _tracer.TrackTrace($"+'{sharePointFileInfo.FullUrl}'...");
            }
        }

        /// <summary>
        /// Checks if a given file in SharePoint exists in blob & has the latest version
        /// </summary>
        public async Task<bool> DoesSharePointFileNeedMigrating(SharePointFileInfo sharePointFileInfo, BlobContainerClient containerClient)
        {
            // Check if blob exists in account
            var fileRef = containerClient.GetBlobClient(sharePointFileInfo.FileRelativePath);
            var fileExistsInAzureBlob = await fileRef.ExistsAsync();

            // Verify version migrated in SQL
            bool logExistsAndIsForSameVersion = false;
            var migratedFile = await _db.Files.Where(f => f.Url.ToLower() == sharePointFileInfo.FullUrl.ToLower()).FirstOrDefaultAsync();
            if (migratedFile != null)
            {
                var log = await _db.FileMigrationsCompleted.Where(l => l.File == migratedFile).SingleOrDefaultAsync();
                if (log != null)
                {
                    logExistsAndIsForSameVersion = log.File.LastModified == sharePointFileInfo.LastModified;
                }
            }
            bool haveRightFile = logExistsAndIsForSameVersion && fileExistsInAzureBlob;

            return !haveRightFile;
        }

        /// <summary>
        /// Download from SP and upload to blob-storage
        /// </summary>
        public async Task<long> MigrateFromSharePointToBlobStorage(SharePointFileInfo fileToMigrate, ClientContext ctx)
        {
            // Download from SP to local
            var downloader = new SharePointFileDownloader(ctx, _config, _tracer);
            var tempFileNameAndSize = await downloader.DownloadFileToTempDir(fileToMigrate);

            // Index file properties - EDIT: ignore for now. Search indexing to be done directly on the blobs
            //var searchIndexer = new SharePointFileSearchProcessor(_config, _tracer);
            //await searchIndexer.ProcessFileContent(msg);

            // Upload local file to az blob
            var blobUploader = new BlobStorageUploader(_config, _tracer);
            await blobUploader.UploadFileToAzureBlob(tempFileNameAndSize.Item1, fileToMigrate);

            // Clean-up temp file
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

        public async Task SaveSucessfulFileMigrationToSql(SharePointFileInfo fileMigrated)
        {
            var migratedFile = await GetDbFileForFileInfo(fileMigrated);

            // Update file last modified
            migratedFile.LastModified = fileMigrated.LastModified;

            // Add log
            var log = await _db.FileMigrationsCompleted.Where(l => l.File == migratedFile).SingleOrDefaultAsync();
            if (log == null)
            {
                log = new FileMigrationCompletedLog { File = migratedFile };
                _db.FileMigrationsCompleted.Add(log);
            }
            log.Migrated = DateTime.Now;
            await _db.SaveChangesAsync();
        }

        public async Task SaveErrorForFileMigrationToSql(Exception ex, SharePointFileInfo fileNotMigrated)
        {
            var errorFile = await GetDbFileForFileInfo(fileNotMigrated);

            // Add log
            var log = await _db.FileMigrationErrors.Where(l => l.File == errorFile).SingleOrDefaultAsync();
            if (log == null)
            {
                log = new FileMigrationErrorLog { File = errorFile };
                _db.FileMigrationErrors.Add(log);
            }
            log.TimeStamp = DateTime.Now;

            await _db.SaveChangesAsync();
        }
        async Task<Entities.DBEntities.File> GetDbFileForFileInfo(SharePointFileInfo fileMigrated)
        {
            // Find/create web & site
            var fileSite = await _db.Sites.Where(f => f.Url.ToLower() == fileMigrated.SiteUrl.ToLower()).FirstOrDefaultAsync();
            if (fileSite == null)
            {
                fileSite = new Entities.DBEntities.Site
                {
                    Url = fileMigrated.SiteUrl.ToLower()
                };
                _db.Sites.Append(fileSite);
            }

            var fileWeb = await _db.Webs.Where(f => f.Url.ToLower() == fileMigrated.WebUrl.ToLower()).FirstOrDefaultAsync();
            if (fileWeb == null)
            {
                fileWeb = new Entities.DBEntities.Web
                {
                    Url = fileMigrated.WebUrl.ToLower(),
                    Site = fileSite
                };
                _db.Webs.Append(fileWeb);
            }

            // Find/create file
            var migratedFileRecord = await _db.Files.Where(f => f.Url.ToLower() == fileMigrated.FullUrl.ToLower()).FirstOrDefaultAsync();
            if (migratedFileRecord == null)
            {
                migratedFileRecord = new Entities.DBEntities.File
                {
                    Url = fileMigrated.FullUrl.ToLower(),
                    Web = fileWeb
                };
                _db.Files.Append(migratedFileRecord);
            }
            migratedFileRecord.LastModified = fileMigrated.LastModified;
            migratedFileRecord.LastModifiedBy = fileMigrated.Author;

            return migratedFileRecord;
        }


        public void Dispose()
        {
            _db.Dispose(); 
        }
    }
}
