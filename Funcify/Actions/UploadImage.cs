using Funcify.Contracts.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Funcify.Actions
{
    public class UploadImage
    {
        private readonly IBlobService _blobService;

        public UploadImage(IBlobService blobService)
        {
            _blobService = blobService;
        }

        public async Task<string> Invoke(string containerName, string fileName, Stream fileStream)
        {
            ValidateInputs(containerName, fileName, fileStream);
            return await _blobService.UploadBlob(containerName, fileName, fileStream);
        }

        #region Private Helper Methods

        private void ValidateInputs(string containerName, string fileName, Stream fileStream)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("Container name cannot be null or empty.", nameof(containerName));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));
        }

        #endregion
    }
}
