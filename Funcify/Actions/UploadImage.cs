using System;
using Azure.Storage.Blobs.Models;
using Funcify.Contracts.Services;
using Funcify.Services;

namespace Funcify.Actions;

public class UploadImage
{
    private readonly IBlobService _blobService;

    public UploadImage(IBlobService blobService) {
        _blobService = blobService;
    }

    public Task<BlobContentInfo> Invoke(string containerName, string fileName, Stream fileStream) {
        throw new NotImplementedException();
    }


}
