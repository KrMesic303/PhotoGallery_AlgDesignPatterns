using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Application.UseCases.Photos.Delete;

namespace PhotoGallery.Application.UseCases.Common.Auditing
{
    /// <summary>
    /// PATTERN: Decorator
    /// </summary>
    public sealed class AuditedDeletePhotoHandler : IDeletePhotoHandler
    {
        private readonly IDeletePhotoHandler _inner;
        private readonly IAuditLogger _audit;

        public AuditedDeletePhotoHandler(IDeletePhotoHandler inner, IAuditLogger audit)
        {
            _inner = inner;
            _audit = audit;
        }

        public async Task HandleAsync(DeletePhotoCommand command, CancellationToken cancellationToken = default)
        {
            await _inner.HandleAsync(command, cancellationToken);

            await _audit.LogAsync(
                userId: command.UserId,
                action: "DELETE_PHOTO",
                entityType: nameof(Photo),
                entityId: command.PhotoId.ToString(),
                cancellationToken: cancellationToken);
        }
    }
}
