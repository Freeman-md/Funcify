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
    private readonly string _databaseContainerName;
    private readonly string _unprocessedBlobsContainerName;
    public ImageResize(IBlobService blobService, ICosmosDBService cosmosDBService)
    {
        _blobService = blobService;
        _cosmosDBService = cosmosDBService;

        _databaseName = "Funcify";
        _databaseContainerName = "Products";

        _unprocessedBlobsContainerName = "unprocessed-images";
    }

    public async Task Invoke(string processedBlobsContainerName, string blobName)
    {
        await this.Invoke(processedBlobsContainerName, "", "", blobName);
    }

    public async Task Invoke(string processedBlobsContainerName, string itemId, string partitionKey, string blobName)
    {
        ValidateInputs(processedBlobsContainerName, blobName);

        string downloadPath = await DownloadBlob(_unprocessedBlobsContainerName, blobName);

        string uploadPath = await ResizeImage(downloadPath);

        string uploadedBlobUri = await UploadResizedImage(processedBlobsContainerName, blobName, uploadPath);

        await UpdateProductImageUrl(itemId, partitionKey, uploadedBlobUri);

        DeleteTempFile(uploadPath, downloadPath);
    }

    // Private methods for each action step
    private void ValidateInputs(string processedBlobsContainerName, string blobName)
    {
        if (string.IsNullOrEmpty(processedBlobsContainerName))
        {
            throw new ArgumentException();
        }

        if (string.IsNullOrEmpty(blobName))
        {
            throw new ArgumentException();
        }
    }

    private async Task<string> DownloadBlob(string unprocessedBlobsContainerName, string blobName)
    {
        return await _blobService.DownloadBlob(unprocessedBlobsContainerName, blobName);
    }

    public async Task<string> ResizeImage(string downloadPath)
    {
        using (Image image = Image.Load(downloadPath))
        {
            image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));

            string uploadPath = Path.Combine(Path.GetDirectoryName(downloadPath)!, "resized_" + Path.GetFileName(downloadPath));

            await image.SaveAsync(uploadPath);

            return uploadPath;
        }
    }

    private async Task<string> UploadResizedImage(string processedBlobsContainerName, string blobName, string uploadPath)
    {
        using (FileStream fileStream = new FileStream(uploadPath, FileMode.Open))
        {
            return await _blobService.UploadBlob(processedBlobsContainerName, blobName, fileStream);
        }
    }

    private async Task UpdateProductImageUrl(string imageId, string partitionKey, string newImageUrl)
    {
        // Build a dictionary of fields to update
        var updates = new Dictionary<string, object>
            {
                { "ProcessedImageUrl", newImageUrl }
            };

        await _cosmosDBService.UpdateItemFields<Product>(_databaseName, _databaseContainerName, imageId, partitionKey, updates);
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
