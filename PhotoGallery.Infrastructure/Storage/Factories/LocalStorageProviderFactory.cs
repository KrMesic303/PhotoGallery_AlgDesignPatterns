using Microsoft.AspNetCore.Hosting;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Storage;

namespace PhotoGallery.Infrastructure.Storage.Factories
{
    public sealed class LocalStorageProviderFactory : IStorageProviderFactory
    {
        private readonly IWebHostEnvironment _env;

        public LocalStorageProviderFactory(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IPhotoStorageService CreatePhotoStorageService() => new LocalPhotoStorageService(_env);
    }
}
