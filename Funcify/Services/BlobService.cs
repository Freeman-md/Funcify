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
        ValidateInputs(containerName, blobName);

        string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), containerName, blobName);

        Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));

        BlobContainerClient blobContainerClient = await GetContainer(containerName);
        BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync()) {
            throw new FileNotFoundException();
        }

        await blobClient.DownloadToAsync(downloadPath);

        return downloadPath;
    }

    private void ValidateInputs(string containerName, string blobName) {
        if (string.IsNullOrEmpty(containerName)) {
            throw new ArgumentException();
        }

        if (string.IsNullOrEmpty(blobName)) {
            throw new ArgumentException();
        }
    }
}