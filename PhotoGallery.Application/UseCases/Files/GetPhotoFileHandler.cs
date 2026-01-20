using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Repositories;

namespace PhotoGallery.Application.UseCases.Files
{
    public sealed class GetPhotoFileHandler : IGetPhotoFileHandler
    {
        private readonly IPhotoRepository _photos;
        private readonly IPhotoStorageService _storage;

        public GetPhotoFileHandler(IPhotoRepository photos, IPhotoStorageService storage)
        {
            _photos = photos;
            _storage = storage;
        }

        public async Task<GetPhotoFileResult> HandleAsync(GetPhotoFileQuery query, CancellationToken cancellationToken = default)
        {
            var photo = await _photos.FindAsync(query.PhotoId, cancellationToken);
            if (photo == null)
                throw new KeyNotFoundException("Photo not found.");

            if (query.IsThumbnail)
            {
                if (string.IsNullOrEmpty(photo.ThumbnailStorageKey))
                    throw new KeyNotFoundException("Thumbnail not found.");

                var thumbStream = await _storage.GetAsync(photo.ThumbnailStorageKey, cancellationToken);

                return new GetPhotoFileResult
                {
                    Stream = thumbStream,
                    ContentType = "image/jpeg"
                };
            }

            var stream = await _storage.GetAsync(photo.StorageKey, cancellationToken);

            return new GetPhotoFileResult
            {
                Stream = stream,
                ContentType = photo.ContentType
            };
        }
    }
}
