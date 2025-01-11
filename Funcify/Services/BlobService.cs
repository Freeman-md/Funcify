using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Funcify.Contracts.Services;

namespace Funcify.Services;

public class BlobService : IBlobService {
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(BlobServiceClient blobServiceClient) {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<BlobContainerClient> GetContainer(string containerName)
    {
        if (string.IsNullOrEmpty(containerName))
            throw new ArgumentException(nameof(containerName));

        BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        if (!await blobContainerClient.ExistsAsync()) {
            await blobContainerClient.CreateIfNotExistsAsync();
        }

        return blobContainerClient;
    }

    public async Task<Response<BlobContentInfo>> UploadBlob(string containerName, string blobName, dynamic data)
    {
        if (string.IsNullOrEmpty(containerName))
            throw new ArgumentException(nameof(containerName));

        if (string.IsNullOrEmpty(blobName))
            throw new ArgumentException(nameof(blobName));

        if (data == null) 
            throw new ArgumentException(nameof(data));

        BlobContainerClient blobContainerClient = await GetContainer(containerName);

        BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

        Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

        return await blobClient.UploadAsync(data, true);

    }
}