using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Application.UseCases.Photos.Upload;

namespace PhotoGallery.Application.UseCases.Common.Auditing
{
    /// <summary>
    /// PATTERN: Decorator
    /// </summary>
    public sealed class AuditedUploadPhotoHandler : IUploadPhotoHandler
    {
        private readonly IUploadPhotoHandler _inner;
        private readonly IAuditLogger _audit;

        public AuditedUploadPhotoHandler(IUploadPhotoHandler inner, IAuditLogger audit)
        {
            _inner = inner;
            _audit = audit;
        }

        public async Task<UploadPhotoResult> HandleAsync(UploadPhotoCommand command, CancellationToken cancellationToken = default)
        {
            var result = await _inner.HandleAsync(command, cancellationToken);

            await _audit.LogAsync(
                userId: command.User.Id,
                action: "UPLOAD_PHOTO",
                entityType: nameof(Photo),
                entityId: result.PhotoId.ToString(),
                cancellationToken: cancellationToken);

            return result;
        }
    }
}
