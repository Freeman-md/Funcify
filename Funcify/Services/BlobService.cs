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
        BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        if (!await blobContainerClient.ExistsAsync()) {
            await blobContainerClient.CreateIfNotExistsAsync();
        }

        return blobContainerClient;
    }

    public async Task<string> UploadBlob(string containerName, string blobName, Stream data)
    {
        BlobContainerClient blobContainerClient = await GetContainer(containerName);

        BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

        Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

        await blobClient.UploadAsync(data, true);

        return blobClient.Uri.ToString();
    }

    public async Task<string> DownloadBlob(string containerName, string blobName) {
        throw new NotImplementedException();
    }
}