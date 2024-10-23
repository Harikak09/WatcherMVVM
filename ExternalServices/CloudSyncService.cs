/******************************************************************************
* Filename    = CloudSyncService.cs
*
* Author      = Karumudi Harika
*
* Product     = Updater.Client
* 
* Project     = External Service
*
* Description = Does the cloud sync of analyzer files.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace FileWatcherMVVM1.ExternalServices
{
    /// <summary>
    /// Provides methods for uploading and deleting files in Azure Blob Storage.
    /// </summary>
    public class CloudSyncService
    {
        private readonly BlobContainerClient _blobContainerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudSyncService"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string to Azure Blob Storage.</param>
        /// <param name="containerName">The name of the Blob container where files will be stored.</param>
        public CloudSyncService(string connectionString, string containerName)
        {
            // Initialize Azure Blob Storage client
            _blobContainerClient = new BlobContainerClient(connectionString, containerName);
        }

        /// <summary>
        /// Uploads a file to the cloud storage asynchronously.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Deletes a file from the cloud storage asynchronously.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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
