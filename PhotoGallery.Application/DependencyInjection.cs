using Microsoft.Extensions.DependencyInjection;
using PhotoGallery.Application.UseCases.Admin.BulkDeletePhotos;
using PhotoGallery.Application.UseCases.Admin.DeletePhoto;
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
            // Photo handlers
            services.AddScoped<IUploadPhotoHandler, UploadPhotoHandler>();
            services.AddScoped<IEditPhotoMetadataHandler, EditPhotoMetadataHandler>();
            services.AddScoped<IDeletePhotoHandler, DeletePhotoHandler>();
            services.AddScoped<IDownloadPhotoHandler, DownloadPhotoHandler>();
            services.AddScoped<IGetEditPhotoHandler, GetEditPhotoHandler>();
            services.AddScoped<IGetPhotoFileHandler, GetPhotoFileHandler>();
            services.AddScoped<IAdminDeletePhotoHandler, AdminDeletePhotoHandler>();

            // Admin handlers implemented in Application
            services.AddScoped<IBulkDeletePhotosHandler, BulkDeletePhotosHandler>();

            return services;
        }
    }
}
