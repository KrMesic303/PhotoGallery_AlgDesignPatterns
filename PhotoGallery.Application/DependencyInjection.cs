using Microsoft.Extensions.DependencyInjection;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.UseCases.Admin.BulkDeletePhotos;
using PhotoGallery.Application.UseCases.Admin.DeletePhoto;
using PhotoGallery.Application.UseCases.Common.Auditing;
using PhotoGallery.Application.UseCases.Files;
using PhotoGallery.Application.UseCases.Photos.Delete;
using PhotoGallery.Application.UseCases.Photos.Download;
using PhotoGallery.Application.UseCases.Photos.Edit;
using PhotoGallery.Application.UseCases.Photos.EditRead;
using PhotoGallery.Application.UseCases.Photos.Upload;

namespace PhotoGallery.Application
{
    /// <summary>
    /// PATTERN: Composition root
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Command pattern registration
            // Inner handlers (concrete implementation)
            services.AddScoped<UploadPhotoHandler>();
            services.AddScoped<EditPhotoMetadataHandler>();
            services.AddScoped<DeletePhotoHandler>();
            services.AddScoped<DownloadPhotoHandler>();

            services.AddScoped<BulkDeletePhotosHandler>();
            services.AddScoped<AdminDeletePhotoHandler>();

            services.AddScoped<GetEditPhotoHandler>();
            services.AddScoped<GetPhotoFileHandler>();

            // Decorator pattern registration
            // Decorated registrations (interfaces - decorator wrapping inner)
            services.AddScoped<IUploadPhotoHandler>(sp => new AuditedUploadPhotoHandler(sp.GetRequiredService<UploadPhotoHandler>(), sp.GetRequiredService<IAuditLogger>()));
            services.AddScoped<IEditPhotoMetadataHandler>(sp => new AuditedEditPhotoMetadataHandler(sp.GetRequiredService<EditPhotoMetadataHandler>(), sp.GetRequiredService<IAuditLogger>()));
            services.AddScoped<IDeletePhotoHandler>(sp => new AuditedDeletePhotoHandler(sp.GetRequiredService<DeletePhotoHandler>(), sp.GetRequiredService<IAuditLogger>()));
            services.AddScoped<IDownloadPhotoHandler>(sp => new AuditedDownloadPhotoHandler(sp.GetRequiredService<DownloadPhotoHandler>(), sp.GetRequiredService<IAuditLogger>()));
            services.AddScoped<IBulkDeletePhotosHandler>(sp => new AuditedBulkDeletePhotosHandler(sp.GetRequiredService<BulkDeletePhotosHandler>(), sp.GetRequiredService<IAuditLogger>()));
            services.AddScoped<IAdminDeletePhotoHandler>(sp => new AuditedAdminDeletePhotoHandler(sp.GetRequiredService<AdminDeletePhotoHandler>(), sp.GetRequiredService<IAuditLogger>()));
            
            // Read/query handlers
            services.AddScoped<IGetEditPhotoHandler>(sp => sp.GetRequiredService<GetEditPhotoHandler>());
            services.AddScoped<IGetPhotoFileHandler>(sp => sp.GetRequiredService<GetPhotoFileHandler>());

            return services;
        }
    }
}
