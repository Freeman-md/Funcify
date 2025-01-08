using Azure.Identity;
using Azure.Storage.Blobs;
using Funcify.Contracts.Services;

namespace Funcify.Services;

public class BlobService : IBlobService {
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(string storageAccountName) {
        string uri = $"https://{storageAccountName}.queue.core.windows.net/";

        _blobServiceClient = new BlobServiceClient(
            new Uri(uri),
            new DefaultAzureCredential()
        );
    }
}