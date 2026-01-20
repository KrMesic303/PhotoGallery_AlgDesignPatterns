using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Application.UseCases.Photos.Download;

namespace PhotoGallery.Application.UseCases.Common.Auditing
{
    /// <summary>
    /// PATTERN: Decorator
    /// </summary>
    public sealed class AuditedDownloadPhotoHandler : IDownloadPhotoHandler
    {
        private readonly IDownloadPhotoHandler _inner;
        private readonly IAuditLogger _audit;

        public AuditedDownloadPhotoHandler(IDownloadPhotoHandler inner, IAuditLogger audit)
        {
            _inner = inner;
            _audit = audit;
        }

        public async Task<DownloadPhotoResult> HandleAsync(DownloadPhotoQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _inner.HandleAsync(query, cancellationToken);

            await _audit.LogAsync(
                userId: query.RequestUserIdOrAnonymous,
                action: "DOWNLOAD_PHOTO",
                entityType: nameof(Photo),
                entityId: query.PhotoId.ToString(),
                cancellationToken: cancellationToken);

            return result;
        }
    }
}
