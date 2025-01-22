using System;
using Funcify.Contracts.Services;

namespace Funcify.Actions;

public class ImageResize
{
    private readonly IBlobService _blobService;
    private readonly UpdateProduct _updateProduct;

    public ImageResize(IBlobService blobService, UpdateProduct updateProduct) {
        _blobService = blobService;
        _updateProduct = updateProduct;
    }

    public async Task Invoke(string containerName, string blobName)
    {
        ValidateInputs(containerName, blobName);

        string downloadPath = await DownloadBlob(containerName, blobName);

        await ResizeImage(downloadPath);

        await UploadResizedImage(containerName, blobName, downloadPath);

        // await UpdateProduct(itemId, downloadPath);

        DeleteTempFile(downloadPath);
    }

    // Private methods for each action step
    private void ValidateInputs(string containerName, string blobName)
    {
        if (string.IsNullOrEmpty(containerName)) {
            throw new ArgumentException();
        }

        if (string.IsNullOrEmpty(blobName)) {
            throw new ArgumentException();
        }
    }

    private async Task<string> DownloadBlob(string containerName, string blobName)
    {
        return await _blobService.DownloadBlob(containerName, blobName);
    }

    private async Task ResizeImage(string downloadPath)
    {
        // TODO: Resize the image from the download path
        throw new NotImplementedException();
    }

    private async Task UploadResizedImage(string containerName, string blobName, string downloadPath)
    {
        // TODO: Use the blob service to upload the resized image
        throw new NotImplementedException();
    }

    private async Task UpdateProduct(string imageId, string downloadPath)
    {
        // TODO: Update Product In Cosmos DB with the new processed image URL
        throw new NotImplementedException();
    }

    private void DeleteTempFile(string downloadPath)
    {
        // TODO: Delete the temporary file from the local system
        throw new NotImplementedException();
    }
}
