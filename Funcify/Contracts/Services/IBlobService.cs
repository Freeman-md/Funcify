using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Funcify.Contracts.Services;

public interface IBlobService {
    public Task<BlobContainerClient> GetContainer(string containerName); 

    public Task<string> UploadBlob(string containerName, string blobName, Stream data);
}