using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Repositories;
using PhotoGallery.Application.Events;
using PhotoGallery.Application.Events.Photos;

namespace PhotoGallery.Application.UseCases.Photos.Delete
{
    public sealed class DeletePhotoHandler(IPhotoRepository photos, IPhotoStorageService storage, IEventPublisher events) : IDeletePhotoHandler
    {
        private readonly IPhotoRepository _photos = photos;
        private readonly IPhotoStorageService _storage = storage;
        private readonly IEventPublisher _events = events;

        public async Task HandleAsync(DeletePhotoCommand command, CancellationToken cancellationToken = default)
        {
            var photo = await _photos.FindAsync(command.PhotoId, cancellationToken) ?? throw new KeyNotFoundException("Photo not found.");
            if (photo.UserId != command.UserId && !command.IsAdmin)
                throw new UnauthorizedAccessException("Forbidden.");

            // Storage cleanup
            await _storage.DeleteAsync(photo.StorageKey, cancellationToken);
            if (!string.IsNullOrEmpty(photo.ThumbnailStorageKey))
                await _storage.DeleteAsync(photo.ThumbnailStorageKey, cancellationToken);

            _photos.Remove(photo);
            await _photos.SaveChangesAsync(cancellationToken);
            await _events.PublishAsync(new PhotoDeletedEvent(photo.Id, command.UserId, command.IsAdmin), cancellationToken);
        }
    }
}
