using Microsoft.AspNetCore.Hosting;
using PhotoGallery.Application.Abstractions;

namespace PhotoGallery.Infrastructure.Storage
{
    public sealed class LocalPhotoStorageService : IPhotoStorageService
    {
        private readonly string _rootPath;

        public LocalPhotoStorageService(IWebHostEnvironment env)
        {
            _rootPath = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(_rootPath);
        }

        public async Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken ct)
        {
            var key = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var path = Path.Combine(_rootPath, key);

            using var fs = new FileStream(path, FileMode.Create);
            await stream.CopyToAsync(fs, ct);

            return key;
        }

        public Task<Stream> GetAsync(string storageKey, CancellationToken ct)
        {
            var path = Path.Combine(_rootPath, storageKey);
            return Task.FromResult<Stream>(File.OpenRead(path));
        }

        public Task DeleteAsync(string storageKey, CancellationToken ct)
        {
            File.Delete(Path.Combine(_rootPath, storageKey));
            return Task.CompletedTask;
        }
    }
}
