using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Application.UseCases.Photos.Edit;

namespace PhotoGallery.Application.UseCases.Common.Auditing
{
    /// <summary>
    /// PATTERN: Decorator
    /// </summary>
    public sealed class AuditedEditPhotoMetadataHandler(IEditPhotoMetadataHandler inner, IAuditLogger audit) : IEditPhotoMetadataHandler
    {
        private readonly IEditPhotoMetadataHandler _inner = inner;
        private readonly IAuditLogger _audit = audit;

        public async Task HandleAsync(EditPhotoMetadataCommand command, CancellationToken cancellationToken = default)
        {
            await _inner.HandleAsync(command, cancellationToken);

            await _audit.LogAsync(
                userId: command.UserId,
                action: "EDIT_PHOTO_METADATA",
                entityType: nameof(Photo),
                entityId: command.PhotoId.ToString(),
                cancellationToken: cancellationToken);
        }
    }
}
