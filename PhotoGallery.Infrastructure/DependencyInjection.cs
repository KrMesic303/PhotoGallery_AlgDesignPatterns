using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Queries;
using PhotoGallery.Application.Abstractions.Repositories;
using PhotoGallery.Infrastructure.DbContext;
using PhotoGallery.Infrastructure.ImageProcessing;
using PhotoGallery.Infrastructure.Logging;
using PhotoGallery.Infrastructure.Queries;
using PhotoGallery.Infrastructure.Queries.Admin;
using PhotoGallery.Infrastructure.Queries.Profile;
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

            // Repositories
            services.AddScoped<IPhotoRepository, EfPhotoRepository>();
            services.AddScoped<IHashtagRepository, EfHashtagRepository>();

            // Query services
            services.AddScoped<IAdminUserQueryService, AdminUserQueryService>();
            services.AddScoped<IAdminPhotoQueryService, AdminPhotoQueryService>();
            services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();
            services.AddScoped<IAdminStatisticsQueryService, AdminStatisticsQueryService>();
            services.AddScoped<IUserProfileQueryService, UserProfileQueryService>();

            // Infrastructure services
            services.AddScoped<IImageTransformService, ImageSharpTransformService>();
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
