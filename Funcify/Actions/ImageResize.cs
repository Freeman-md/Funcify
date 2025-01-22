using Funcify.Contracts.Services;
using Funcify.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Funcify.Actions
{
    public class ImageResize
    {
        private readonly IBlobService _blobService;
        private readonly ICosmosDBService _cosmosDBService;

        private const string DatabaseName = "Funcify";
        private const string ContainerName = "Products";
        private const string UnprocessedBlobsContainer = "unprocessed-images";

        public ImageResize(IBlobService blobService, ICosmosDBService cosmosDBService)
        {
            _blobService = blobService;
            _cosmosDBService = cosmosDBService;
        }

        public async Task Invoke(string processedBlobsContainerName, string blobName)
        {
            await Invoke(processedBlobsContainerName, string.Empty, string.Empty, blobName);
        }

        public async Task Invoke(string processedBlobsContainerName, string itemId, string partitionKey, string blobName)
        {
            ValidateInputs(processedBlobsContainerName, blobName);

            string downloadPath = await _blobService.DownloadBlob(UnprocessedBlobsContainer, blobName);

            string resizedImagePath = await ResizeImage(downloadPath);

            // Re-open the resized file to upload
            using var resizedStream = File.OpenRead(resizedImagePath);
            string uploadedBlobUri = await _blobService.UploadBlob(processedBlobsContainerName, blobName, resizedStream);

            // If we have valid itemId and partitionKey, update Cosmos DB record
            await UpdateProductImageUrl(itemId, partitionKey, uploadedBlobUri);

            DeleteTempFile(resizedImagePath);
            DeleteTempFile(downloadPath);
        }

        #region Private Helper Methods

        private void ValidateInputs(string processedBlobsContainerName, string blobName)
        {
            if (string.IsNullOrWhiteSpace(processedBlobsContainerName))
                throw new ArgumentException("Processed container name cannot be null or empty.", nameof(processedBlobsContainerName));

            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));
        }

        private async Task<string> ResizeImage(string imagePath)
        {
            using (Image image = Image.Load(imagePath))
            {
                image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));

                string resizedFileName = "resized_" + Path.GetFileName(imagePath);
                string resizedImagePath = Path.Combine(Path.GetDirectoryName(imagePath) ?? string.Empty, resizedFileName);

                await image.SaveAsync(resizedImagePath);
                return resizedImagePath;
            }
        }

        private async Task UpdateProductImageUrl(string itemId, string partitionKey, string newImageUrl)
        {
            // Only update if we actually have an ID and partition key
            if (!string.IsNullOrEmpty(itemId) && !string.IsNullOrEmpty(partitionKey))
            {
                var updates = new Dictionary<string, object>
                {
                    { nameof(Product.ProcessedImageUrl), newImageUrl }
                };

                await _cosmosDBService.UpdateItemFields<Product>(
                    DatabaseName,
                    ContainerName,
                    itemId,
                    partitionKey,
                    updates
                );
            }
        }

        private void DeleteTempFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        #endregion
    }
}
