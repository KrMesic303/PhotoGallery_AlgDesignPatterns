using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Repositories;
using PhotoGallery.Application.Events;
using PhotoGallery.Application.Events.Photos;

namespace PhotoGallery.Application.UseCases.Photos.Download
{
    public sealed class DownloadPhotoHandler : IDownloadPhotoHandler
    {
        private readonly IPhotoRepository _photos;
        private readonly IPhotoStorageService _storage;
        private readonly IImageTransformService _transform;
        private readonly IEventPublisher _events;

        public DownloadPhotoHandler(
            IPhotoRepository photos,
            IPhotoStorageService storage,
            IImageTransformService transform,
            IEventPublisher events)
        {
            _photos = photos;
            _storage = storage;
            _transform = transform;
            _events = events;
        }

        public async Task<DownloadPhotoResult> HandleAsync(DownloadPhotoQuery query, CancellationToken cancellationToken = default)
        {
            var photo = await _photos.FindAsync(query.PhotoId, cancellationToken);
            if (photo == null)
                throw new KeyNotFoundException("Photo not found.");

            if (query.DownloadOriginal)
            {
                // Do NOT dispose this stream here - its disposed later by ASP.NET
                var originalStream = await _storage.GetAsync(photo.StorageKey, cancellationToken);

                await _events.PublishAsync(new PhotoDownloadedEvent(photo.Id, query.RequestUserIdOrAnonymous, query.DownloadOriginal), cancellationToken);

                return new DownloadPhotoResult
                {
                    Stream = originalStream,
                    ContentType = photo.ContentType,
                    FileName = photo.OriginalFileName
                };
            }

            await using var source = await _storage.GetAsync(photo.StorageKey, cancellationToken);

            var transformed = await _transform.TransformForDownloadAsync(
                source,
                photo.OriginalFileName,
                query.Options,
                cancellationToken);

            // Do NOT dispose transformed.ImageStream before returning.
            var fileName = Path.GetFileNameWithoutExtension(photo.OriginalFileName) + transformed.ImageExtension;

            await _events.PublishAsync(new PhotoDownloadedEvent(photo.Id, query.RequestUserIdOrAnonymous, query.DownloadOriginal), cancellationToken);

            return new DownloadPhotoResult
            {
                Stream = transformed.ImageStream,
                ContentType = transformed.ImageContentType,
                FileName = fileName
            };
        }
    }
}
