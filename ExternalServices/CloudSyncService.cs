using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace FileWatcherMVVM1.ExternalServices
{
    public class CloudSyncService
    {
        private readonly BlobContainerClient _blobContainerClient;

        public CloudSyncService(string connectionString, string containerName)
        {
            // Initialize Azure Blob Storage client
            _blobContainerClient = new BlobContainerClient(connectionString, containerName);
        }

        public async Task UploadFileToCloud(string filePath, string fileName)
        {
            try
            {
                BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    await blobClient.UploadAsync(fileStream, overwrite: true);
                }
            }
            catch (Exception ex)
            {
                // Handle upload error
                throw new InvalidOperationException($"Failed to upload file '{fileName}'", ex);
            }
        }

        public async Task DeleteFileFromCloud(string fileName)
        {
            try
            {
                BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                // Handle delete error
                throw new InvalidOperationException($"Failed to delete file '{fileName}'", ex);
            }
        }
    }

}
