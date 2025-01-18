using System;
using Azure.Storage.Blobs.Models;
using Funcify.Contracts.Services;
using Funcify.Services;

namespace Funcify.Actions;

public class UploadImage
{
    private readonly IBlobService _blobService;

    public UploadImage(IBlobService blobService)
    {
        _blobService = blobService;
    }

    public async Task<BlobContentInfo> Invoke(string containerName, string fileName, Stream fileStream)
    {
        ValidateInputs(containerName, fileName, fileStream);

        return await _blobService.UploadBlob(containerName, fileName, fileStream);
    }

    private void ValidateInputs(string containerName, string fileName, Stream fileStream)
    {
        if (string.IsNullOrEmpty(containerName))
            throw new ArgumentException(nameof(containerName));

        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException(nameof(fileName));

        if (fileStream == null)
            throw new ArgumentException(nameof(fileStream));
    }


}
