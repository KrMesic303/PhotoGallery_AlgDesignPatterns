using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Repositories;

namespace PhotoGallery.Application.UseCases.Admin.DeletePhoto
{
    public sealed class AdminDeletePhotoHandler(IPhotoRepository photos, IPhotoStorageService storage) : IAdminDeletePhotoHandler
    {
        private readonly IPhotoRepository _photos = photos;
        private readonly IPhotoStorageService _storage = storage;

        public async Task HandleAsync(AdminDeletePhotoCommand command, CancellationToken cancellationToken = default)
        {
            var photo = await _photos.FindAsync(command.PhotoId, cancellationToken) ?? throw new KeyNotFoundException("Photo not found.");
            await _storage.DeleteAsync(photo.StorageKey, cancellationToken);
            if (!string.IsNullOrEmpty(photo.ThumbnailStorageKey))
                await _storage.DeleteAsync(photo.ThumbnailStorageKey, cancellationToken);

            _photos.Remove(photo);
            await _photos.SaveChangesAsync(cancellationToken);

        }
    }
}
