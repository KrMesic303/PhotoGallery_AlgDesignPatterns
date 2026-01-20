using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Queries;
using PhotoGallery.Application.Abstractions.Repositories;

namespace PhotoGallery.Application.UseCases.Admin.BulkDeletePhotos
{
    public sealed class BulkDeletePhotosHandler : IBulkDeletePhotosHandler
    {
        private readonly IAdminPhotoQueryService _adminPhotos;
        private readonly IPhotoRepository _photos;
        private readonly IPhotoStorageService _storage;

        public BulkDeletePhotosHandler(
            IAdminPhotoQueryService adminPhotos,
            IPhotoRepository photos,
            IPhotoStorageService storage)
        {
            _adminPhotos = adminPhotos;
            _photos = photos;
            _storage = storage;
        }

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
