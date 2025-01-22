using System;
using Funcify.Contracts.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Funcify.Actions;

public class ImageResize
{
    private readonly IBlobService _blobService;
    private readonly ICosmosDBService _cosmosDBService;
    private readonly string _databaseName;
    private readonly string _containerName;
    public ImageResize(IBlobService blobService, ICosmosDBService cosmosDBService)
    {
        _blobService = blobService;
        _cosmosDBService = cosmosDBService;

        _databaseName = "Funcify";
        _containerName = "Products";
    }

    public async Task Invoke(string containerName, string blobUri)
    {
        await this.Invoke(containerName, "", "", blobUri);
    }

    public async Task Invoke(string containerName, string itemId, string partitionKey, string blobUri)
    {
        ValidateInputs(containerName, blobUri);

        string downloadPath = await DownloadBlob(containerName, blobUri);

        string uploadPath = await ResizeImage(downloadPath);

        string uploadedBlobUri = await UploadResizedImage(containerName, blobUri, uploadPath);

        await UpdateProductImageUrl(itemId, partitionKey, uploadedBlobUri);

        DeleteTempFile(uploadPath, downloadPath);
    }

    // Private methods for each action step
    private void ValidateInputs(string containerName, string blobUri)
    {
        if (string.IsNullOrEmpty(containerName))
        {
            throw new ArgumentException();
        }

        if (string.IsNullOrEmpty(blobUri))
        {
            throw new ArgumentException();
        }
    }

    private async Task<string> DownloadBlob(string containerName, string blobUri)
    {
        return await _blobService.DownloadBlob(containerName, blobUri);
    }

    public async Task<string> ResizeImage(string downloadPath)
    {
        using (Image image = Image.Load(downloadPath))
        {
            image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));

            string uploadPath = Path.Combine(Path.GetDirectoryName(downloadPath), "resized_" + Path.GetFileName(downloadPath));

            await image.SaveAsync(uploadPath);

            return uploadPath;
        }
    }

    private async Task<string> UploadResizedImage(string containerName, string blobUri, string uploadPath)
    {
        using (FileStream fileStream = new FileStream(uploadPath, FileMode.Open))
        {
            return await _blobService.UploadBlob(containerName, blobUri, fileStream);
        }
    }

    private async Task UpdateProductImageUrl(string imageId, string partitionKey, string newImageUrl)
    {
        // Build a dictionary of fields to update
        var updates = new Dictionary<string, object>
            {
                { "ProcessedImageUrl", newImageUrl }
            };

        // Use the CosmosDBService to update the product's image URL
        await _cosmosDBService.UpdateItemFields<Product>("Database", "Container", imageId, partitionKey, updates);
    }

    private void DeleteTempFile(string uploadPath, string downloadPath)
    {
        if (File.Exists(uploadPath))
        {
            File.Delete(uploadPath);
        }

        if (File.Exists(downloadPath))
        {
            File.Delete(downloadPath);
        }
    }
}
