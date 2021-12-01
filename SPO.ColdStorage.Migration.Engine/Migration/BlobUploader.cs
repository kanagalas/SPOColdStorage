using Azure.Storage.Blobs;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Migration.Engine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine.Migration
{
    public class BlobUploader : BaseComponent
    {
        private BlobServiceClient _blobServiceClient;
        private BlobContainerClient? _containerClient;
        public BlobUploader(Config config) : base(config)
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

            using (var fs = File.OpenRead(localTempFileName))
            {
                await _containerClient.UploadBlobAsync(msg.FileRelativePath, fs);
            }
        }
    }
}
