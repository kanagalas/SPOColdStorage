using Azure.Storage.Blobs;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Migration.Engine.Model;

namespace SPO.ColdStorage.Migration.Engine.Migration
{
    public class BlobStorageUploader : BaseComponent
    {
        private BlobServiceClient _blobServiceClient;
        private BlobContainerClient? _containerClient;
        public BlobStorageUploader(Config config) : base(config)
        {
            // Create a BlobServiceClient object which will be used to create a container client
            _blobServiceClient = new BlobServiceClient(_config.StorageConnectionString);
        }


        public async Task UploadFileToAzureBlob(string localTempFileName, SharePointFileInfo msg)
        {
            // Create the container and return a container client object
            if (_containerClient == null)
            {
                this._containerClient = _blobServiceClient.GetBlobContainerClient(_config.BlobContainerName);
            }

            _tracer.TrackTrace($"Uploading '{msg.FileRelativePath}' to blob storage...");
            using (var fs = File.OpenRead(localTempFileName))
            {
                var existing = _containerClient.GetBlobClient(msg.FileRelativePath);
                if (await existing.ExistsAsync())
                {
                    byte[] hash;
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        using (var stream = File.OpenRead(localTempFileName))
                        {
                            hash = md5.ComputeHash(stream);
                        }
                    }
                    var existingProps = await existing.GetPropertiesAsync();
                    var match = existingProps.Value.ContentHash.SequenceEqual(hash);
                    if (!match)
                        await existing.UploadAsync(fs, true);
                    else
                        _tracer.TrackTrace($"Skipping '{msg.FileRelativePath}' as destination hash is identical to local file.");
                }
                else
                    await _containerClient.UploadBlobAsync(msg.FileRelativePath, fs);
            }
        }
    }
}
