using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoGallery.Application.Abstractions.Storage;

namespace PhotoGallery.Infrastructure.Storage.Factories
{
    /// <summary>
    /// PATTERN: Abstract factory : selector
    /// </summary>
    public sealed class StorageProviderFactorySelector(IConfiguration config, IServiceProvider sp)
    {
        private readonly IConfiguration _config = config;
        private readonly IServiceProvider _sp = sp;

        public IStorageProviderFactory Select()
        {
            var provider = _config["Storage:Provider"] ?? "Local";

            return provider.Equals("Gcs", StringComparison.OrdinalIgnoreCase)
                ? _sp.GetRequiredService<GcsStorageProviderFactory>()
                : _sp.GetRequiredService<LocalStorageProviderFactory>();
        }
    }
}
