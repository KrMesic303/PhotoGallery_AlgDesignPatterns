using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Queries;
using PhotoGallery.Application.Abstractions.Repositories;

namespace PhotoGallery.Application.UseCases.Admin.BulkDeletePhotos
{
    public sealed class BulkDeletePhotosHandler(
        IAdminPhotoQueryService adminPhotos,
        IPhotoRepository photosRepository,
        IPhotoStorageService storage) : IBulkDeletePhotosHandler
    {
        private readonly IAdminPhotoQueryService _adminPhotos = adminPhotos;
        private readonly IPhotoRepository _photos = photosRepository;
        private readonly IPhotoStorageService _storage = storage;

        public async Task HandleAsync(BulkDeletePhotosCommand command, CancellationToken cancellationToken = default)
        {
            if (command.PhotoIds == null || command.PhotoIds.Length == 0)
                return;

            var photos = await _adminPhotos.GetPhotosByIdsAsync(command.PhotoIds, cancellationToken);

            foreach (var photo in photos)
            {
                await _storage.DeleteAsync(photo.StorageKey, cancellationToken);
                if (!string.IsNullOrEmpty(photo.ThumbnailStorageKey))
                    await _storage.DeleteAsync(photo.ThumbnailStorageKey, cancellationToken);

                _photos.Remove(photo);
            }

            await _photos.SaveChangesAsync(cancellationToken);
        }
    }
}
