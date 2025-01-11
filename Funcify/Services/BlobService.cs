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

    public async Task<BlobContainerClient> CreateContainer(string containerName)
    {
        if (string.IsNullOrEmpty(containerName))
            throw new ArgumentException(nameof(containerName));

        BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        if (!await blobContainerClient.ExistsAsync()) {
            await blobContainerClient.CreateIfNotExistsAsync();
        }

        return blobContainerClient;
    }

    public Task<Response<BlobContentInfo>> UploadBlob(string containerName, string blobName, dynamic data)
    {
        throw new NotImplementedException();
    }
}