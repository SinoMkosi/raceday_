using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    public class BlobStorageService
    {
        private const long MaxBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };

        private readonly BlobContainerClient? _container;

        public BlobStorageService(IConfiguration config)
        {
            var connection = config["AzureStorage:ConnectionString"];
            var containerName = config["AzureStorage:ContainerName"] ?? "raceday-images";

            if (!string.IsNullOrWhiteSpace(connection)
                && !connection.Contains("PASTE_YOUR_CONNECTION_STRING_HERE"))
            {
                _container = new BlobContainerClient(connection, containerName);
            }
        }

        public bool IsConfigured => _container != null;

        public static string? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return "Please choose an image to upload.";
            }

            if (file.Length > MaxBytes)
            {
                return "The image must be 5 MB or smaller.";
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                return "Only JPG, PNG, WEBP or GIF images are allowed.";
            }

            if (!AllowedTypes.Contains(file.ContentType))
            {
                return "That file is not a valid image.";
            }

            return null;
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            await _container!.CreateIfNotExistsAsync();

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var blobName = $"{folder}/{Guid.NewGuid():N}{extension}";
            var blob = _container.GetBlobClient(blobName);

            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobName;
        }

        public async Task<(Stream Content, string ContentType)?> OpenAsync(string blobName)
        {
            var blob = _container!.GetBlobClient(blobName);

            if (!await blob.ExistsAsync())
            {
                return null;
            }

            var download = await blob.DownloadStreamingAsync();
            return (download.Value.Content, download.Value.Details.ContentType);
        }

        public async Task DeleteAsync(string? blobName)
        {
            if (_container == null || string.IsNullOrWhiteSpace(blobName))
            {
                return;
            }

            try
            {
                await _container.GetBlobClient(blobName).DeleteIfExistsAsync();
            }
            catch (Exception)
            {
                // Deleting an old image is not important enough to fail the request.
            }
        }
    }
}