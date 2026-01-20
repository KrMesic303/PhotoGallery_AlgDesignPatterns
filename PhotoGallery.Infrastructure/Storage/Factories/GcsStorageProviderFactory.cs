using Microsoft.Extensions.Configuration;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Storage;

namespace PhotoGallery.Infrastructure.Storage.Factories
{
    public sealed class GcsStorageProviderFactory(IConfiguration config) : IStorageProviderFactory
    {
        private readonly IConfiguration _config = config;

        public IPhotoStorageService CreatePhotoStorageService() => new GcsPhotoStorageService(_config);
    }
}
