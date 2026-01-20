using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Repositories;
using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.UseCases.Admin.DeletePhoto
{
    public sealed class AdminDeletePhotoHandler : IAdminDeletePhotoHandler
    {
        private readonly IPhotoRepository _photos;
        private readonly IPhotoStorageService _storage;
        private readonly IAuditLogger _audit;

        public AdminDeletePhotoHandler(IPhotoRepository photos, IPhotoStorageService storage, IAuditLogger audit)
        {
            _photos = photos;
            _storage = storage;
            _audit = audit;
        }

        public async Task HandleAsync(AdminDeletePhotoCommand command, CancellationToken cancellationToken = default)
        {
            var photo = await _photos.FindAsync(command.PhotoId, cancellationToken);
            if (photo == null)
                throw new KeyNotFoundException("Photo not found.");

            await _storage.DeleteAsync(photo.StorageKey, cancellationToken);
            if (!string.IsNullOrEmpty(photo.ThumbnailStorageKey))
                await _storage.DeleteAsync(photo.ThumbnailStorageKey, cancellationToken);

            _photos.Remove(photo);
            await _photos.SaveChangesAsync(cancellationToken);

            await _audit.LogAsync(
                userId: command.AdminUserId,
                action: "ADMIN_DELETE_PHOTO",
                entityType: nameof(Photo),
                entityId: photo.Id.ToString(),
                cancellationToken: cancellationToken);
        }
    }
}
