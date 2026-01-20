using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Repositories;

namespace PhotoGallery.Application.UseCases.Photos.Delete
{
    public sealed class DeletePhotoHandler : IDeletePhotoHandler
    {
        private readonly IPhotoRepository _photos;
        private readonly IPhotoStorageService _storage;

        public DeletePhotoHandler(IPhotoRepository photos, IPhotoStorageService storage)
        {
            _photos = photos;
            _storage = storage;
        }

        public async Task HandleAsync(DeletePhotoCommand command, CancellationToken cancellationToken = default)
        {
            var photo = await _photos.FindAsync(command.PhotoId, cancellationToken);
            if (photo == null)
                throw new KeyNotFoundException("Photo not found.");

            if (photo.UserId != command.UserId && !command.IsAdmin)
                throw new UnauthorizedAccessException("Forbidden.");

            // Storage cleanup
            await _storage.DeleteAsync(photo.StorageKey, cancellationToken);
            if (!string.IsNullOrEmpty(photo.ThumbnailStorageKey))
                await _storage.DeleteAsync(photo.ThumbnailStorageKey, cancellationToken);

            _photos.Remove(photo);
            await _photos.SaveChangesAsync(cancellationToken);
        }
    }
}
