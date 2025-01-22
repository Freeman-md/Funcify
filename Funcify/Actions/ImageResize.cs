using System;
using Funcify.Contracts.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Funcify.Actions;

public class ImageResize
{
    private readonly IBlobService _blobService;
    private readonly UpdateProduct _updateProduct;

    public ImageResize(IBlobService blobService, UpdateProduct updateProduct)
    {
        _blobService = blobService;
        _updateProduct = updateProduct;
    }

    public async Task Invoke(string containerName, string blobUri)
    {
        ValidateInputs(containerName, blobUri);

        string downloadPath = await DownloadBlob(containerName, blobUri);

        string uploadPath = await ResizeImage(downloadPath);

        await UploadResizedImage(containerName, blobUri, uploadPath);

        // await UpdateProduct(itemId, downloadPath);

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

    private async Task UploadResizedImage(string containerName, string blobUri, string uploadPath)
    {
        using (FileStream fileStream = new FileStream(uploadPath, FileMode.Open))
        {
            await _blobService.UploadBlob(containerName, blobUri, fileStream);
        }
    }

    private async Task UpdateProduct(string imageId, string downloadPath)
    {
        // TODO: Update Product In Cosmos DB with the new processed image URL
        throw new NotImplementedException();
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
