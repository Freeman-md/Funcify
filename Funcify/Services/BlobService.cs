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

    public Task<BlobContainerClient> CreateContainerIfNotExistsAsync(string containerName)
    {
        throw new NotImplementedException();
    }

    public Task<Response<BlobContentInfo>> UploadBlobToContainer(string fileName, string localFilePath)
    {
        throw new NotImplementedException();
    }
}