using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Funcify.Contracts.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Funcify.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<BlobContainerClient> GetContainer(string containerName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Create container if it doesn't exist
            if (!await containerClient.ExistsAsync())
            {
                await containerClient.CreateIfNotExistsAsync();
            }

            return containerClient;
        }

        public async Task<string> UploadBlob(string containerName, string blobName, Stream data)
        {
            BlobContainerClient containerClient = await GetContainer(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}", blobClient.Uri);

            await blobClient.UploadAsync(data, overwrite: true);

            return blobClient.Uri.ToString();
        }

        public async Task<string> DownloadBlob(string containerName, string blobName)
        {
            ValidateInputs(containerName, blobName);

            string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), containerName, Path.GetFileName(blobName));
            Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));

            BlobContainerClient containerClient = await GetContainer(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"Blob '{blobName}' not found in container '{containerName}'.");
            }

            await blobClient.DownloadToAsync(downloadPath);

            return downloadPath;
        }

        #region Private Helper

        private void ValidateInputs(string containerName, string blobName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentException("Container name cannot be null or empty.", nameof(containerName));
            }

            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));
            }
        }

        #endregion
    }
}
