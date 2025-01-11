using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Funcify.Contracts.Services;

public interface IBlobService {
    public Task<BlobContainerClient> CreateContainer(string containerName); 

    public Task<Response<BlobContentInfo>> UploadBlob(string containerName, string blobName, dynamic data);
}