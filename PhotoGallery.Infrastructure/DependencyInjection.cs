using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Metrics;
using PhotoGallery.Application.Abstractions.Queries;
using PhotoGallery.Application.Abstractions.Repositories;
using PhotoGallery.Application.Events;
using PhotoGallery.Application.Events.Photos;
using PhotoGallery.Application.UseCases.Admin.ChangePackage;
using PhotoGallery.Application.UseCases.Common.Auditing;
using PhotoGallery.Infrastructure.DbContext;
using PhotoGallery.Infrastructure.EventHandlers;
using PhotoGallery.Infrastructure.Events;
using PhotoGallery.Infrastructure.ImageProcessing;
using PhotoGallery.Infrastructure.ImageProcessing.Templates;
using PhotoGallery.Infrastructure.Logging;
using PhotoGallery.Infrastructure.Metrics;
using PhotoGallery.Infrastructure.Queries;
using PhotoGallery.Infrastructure.Queries.Admin;
using PhotoGallery.Infrastructure.Queries.Profile;
using PhotoGallery.Infrastructure.Services;
using PhotoGallery.Infrastructure.Storage;
using PhotoGallery.Infrastructure.UseCases.Admin.ChangePackage;

namespace PhotoGallery.Infrastructure
{
    /// <summary>
    /// PATTERN: Composition root
    /// </summary>
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
            services.AddScoped<IAuditLogger, AuditLogger>();

            // Observer
            services.AddScoped<IEventPublisher, InProcessEventPublisher>();
            services.AddScoped<IAppMetricStore, EfAppMetricStore>();

            // Event observers (handlers)
            services.AddScoped<IDomainEventHandler<PhotoUploadedEvent>, PhotoMetricEventHandler>();
            services.AddScoped<IDomainEventHandler<PhotoDownloadedEvent>, PhotoMetricEventHandler>();
            services.AddScoped<IDomainEventHandler<PhotoDeletedEvent>, PhotoMetricEventHandler>();

            // Decorator
            services.AddScoped<ChangePackageHandler>();
            services.AddScoped<IChangePackageHandler>(sp => new AuditedChangePackageHandler(
                sp.GetRequiredService<ChangePackageHandler>(),
                sp.GetRequiredService<IAuditLogger>(),
                sp.GetRequiredService<IEventPublisher>()));

            // Template
            services.AddScoped<StorageImageTransformTemplate>();
            services.AddScoped<DownloadImageTransformTemplate>();
            services.AddScoped<IImageTransformService, ImageSharpTransformService>();

            services.AddScoped<IImageProcessorFactory, ImageProcessorFactory>();
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
