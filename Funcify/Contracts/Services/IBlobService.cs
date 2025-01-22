using Azure.Storage.Blobs;
using System.IO;
using System.Threading.Tasks;

namespace Funcify.Contracts.Services
{
    public interface IBlobService
    {
        Task<BlobContainerClient> GetContainer(string containerName);
        Task<string> UploadBlob(string containerName, string blobName, Stream data);
        Task<string> DownloadBlob(string containerName, string blobName);
    }
}
