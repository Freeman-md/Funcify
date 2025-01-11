using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Funcify.Contracts.Services;

public interface IBlobService {
    public Task<BlobContainerClient> CreateContainer(string containerName); 

    public Task<Response<BlobContentInfo>> UploadBlobToContainer(string fileName, string localFilePath);
}