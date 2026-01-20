using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Application.UseCases.Admin.DeletePhoto;

namespace PhotoGallery.Application.UseCases.Common.Auditing
{
    /// <summary>
    /// PATTERN: Decorator
    /// </summary>
    public sealed class AuditedAdminDeletePhotoHandler : IAdminDeletePhotoHandler
    {
        private readonly IAdminDeletePhotoHandler _inner;
        private readonly IAuditLogger _audit;

        public AuditedAdminDeletePhotoHandler(IAdminDeletePhotoHandler inner, IAuditLogger audit)
        {
            _inner = inner;
            _audit = audit;
        }

        public async Task HandleAsync(AdminDeletePhotoCommand command, CancellationToken cancellationToken = default)
        {
            await _inner.HandleAsync(command, cancellationToken);

            await _audit.LogAsync(
                userId: command.AdminUserId,
                action: "ADMIN_DELETE_PHOTO",
                entityType: nameof(Photo),
                entityId: command.PhotoId.ToString(),
                cancellationToken: cancellationToken);
        }
    }
}
