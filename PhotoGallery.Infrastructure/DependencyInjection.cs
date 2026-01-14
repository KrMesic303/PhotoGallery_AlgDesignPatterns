using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Infrastructure.DbContext;
using PhotoGallery.Infrastructure.ImageProcessing;
using PhotoGallery.Infrastructure.Logging;
using PhotoGallery.Infrastructure.Queries;
using PhotoGallery.Infrastructure.Services;
using PhotoGallery.Infrastructure.Storage;

namespace PhotoGallery.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Infrastructure services
            services.AddScoped<IImageProcessorFactory, ImageProcessorFactory>();
            services.AddScoped<IAuditLogger, AuditLogger>();
            services.AddScoped<IUploadQuotaService, UploadQuotaService>();
            services.AddScoped<IPhotoUploadPolicy, PhotoUploadPolicy>();
            services.AddScoped<IPhotoQueryService, PhotoQueryService>();

            // Storage provider selection
            var provider = configuration["Storage:Provider"] ?? "Local";
            if (provider.Equals("Gcs", StringComparison.OrdinalIgnoreCase))
            {
                services.AddScoped<IPhotoStorageService, GcsPhotoStorageService>();
            }
            else
            {
                services.AddScoped<IPhotoStorageService, LocalPhotoStorageService>();
            }

            return services;
        }
    }
}
