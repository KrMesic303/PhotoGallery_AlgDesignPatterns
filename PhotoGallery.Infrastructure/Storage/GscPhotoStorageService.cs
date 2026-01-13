using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using PhotoGallery.Application.Abstractions;

namespace PhotoGallery.Infrastructure.Storage
{
    public sealed class GcsPhotoStorageService : IPhotoStorageService
    {
        private readonly StorageClient _client;
        private readonly string _bucket;
        private readonly string _prefix;

        public GcsPhotoStorageService(IConfiguration config)
        {
            _client = StorageClient.Create();
            _bucket = config["Storage:Gcs:BucketName"] ?? throw new InvalidOperationException("Missing Storage:Gcs:BucketName");
            _prefix = config["Storage:Gcs:UploadsPrefix"] ?? "uploads";
        }

        private string NormalizeKey(string fileName)
        {
            fileName = fileName.Replace("\\", "/");
            return $"{_prefix}/{fileName}".TrimStart('/');
        }

        public async Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            var objectName = NormalizeKey(fileName);

            await _client.UploadObjectAsync(
                bucket: _bucket,
                objectName: objectName,
                contentType: contentType,
                source: stream);

            return objectName;
        }

        public async Task<Stream> GetAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            var ms = new MemoryStream();
            await _client.DownloadObjectAsync(_bucket, storageKey, ms);
            ms.Position = 0;
            return ms;
        }

        public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            await _client.DeleteObjectAsync(_bucket, storageKey);
        }
    }
}
