using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Application.UseCases.Admin.BulkDeletePhotos;

namespace PhotoGallery.Application.UseCases.Common.Auditing
{
    /// <summary>
    /// PATTERN: Decorator
    /// </summary>
    public sealed class AuditedBulkDeletePhotosHandler : IBulkDeletePhotosHandler
    {
        private readonly IBulkDeletePhotosHandler _inner;
        private readonly IAuditLogger _audit;

        public AuditedBulkDeletePhotosHandler(IBulkDeletePhotosHandler inner, IAuditLogger audit)
        {
            _inner = inner;
            _audit = audit;
        }

        public async Task HandleAsync(BulkDeletePhotosCommand command, CancellationToken cancellationToken = default)
        {
            await _inner.HandleAsync(command, cancellationToken);

            await _audit.LogAsync(
                userId: command.AdminUserId,
                action: "BULK_DELETE_PHOTO",
                entityType: nameof(Photo),
                entityId: string.Join(",", command.PhotoIds ?? Array.Empty<int>()),
                cancellationToken: cancellationToken);
        }
    }
}
