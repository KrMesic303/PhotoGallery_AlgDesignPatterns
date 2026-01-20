using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Repositories;
using PhotoGallery.Application.Events;
using PhotoGallery.Application.Events.Photos;
using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.UseCases.Photos.Upload
{
    public sealed class UploadPhotoHandler(
        IPhotoUploadPolicy uploadPolicy,
        IImageTransformService imageTransform,
        IPhotoStorageService storage,
        IPhotoRepository photos,
        IHashtagRepository hashtags,
        IEventPublisher events) : IUploadPhotoHandler
    {
        private readonly IPhotoUploadPolicy _uploadPolicy = uploadPolicy;
        private readonly IImageTransformService _imageTransform = imageTransform;
        private readonly IPhotoStorageService _storage = storage;
        private readonly IPhotoRepository _photos = photos;
        private readonly IHashtagRepository _hashtags = hashtags;
        private readonly IEventPublisher _events = events;

        public async Task<UploadPhotoResult> HandleAsync(UploadPhotoCommand command, CancellationToken cancellationToken = default)
        {
            // Policy
            var policy = await _uploadPolicy.CanUploadAsync(command.User, command.FileSizeBytes, cancellationToken);
            if (!policy.IsAllowed)
                throw new InvalidOperationException(policy.ErrorMessage ?? "Upload not allowed.");

            // Transform for storage
            await using var transformed = await _imageTransform.TransformForStorageAsync(
                command.FileStream,
                command.OriginalFileName,
                command.Options,
                cancellationToken);

            var baseName = Path.GetFileNameWithoutExtension(command.OriginalFileName);
            var storedFileName = baseName + transformed.ImageExtension;

            var storageKey = await _storage.SaveAsync(
                transformed.ImageStream,
                storedFileName,
                transformed.ImageContentType,
                cancellationToken);

            var thumbKey = string.Empty;
            if (transformed.ThumbnailStream != null)
            {
                var thumbName = "thumb_" + baseName + (transformed.ThumbnailExtension ?? ".jpg");
                thumbKey = await _storage.SaveAsync(
                    transformed.ThumbnailStream,
                    thumbName,
                    transformed.ThumbnailContentType ?? "image/jpeg",
                    cancellationToken);
            }

            var photo = new Photo
            {
                StorageKey = storageKey,
                ThumbnailStorageKey = thumbKey,
                OriginalFileName = command.OriginalFileName,
                ContentType = transformed.ImageContentType,
                SizeInBytes = transformed.ImageStream.Length,
                Description = command.Description ?? string.Empty,
                UserId = command.User.Id
            };

            // Filters
            if (command.Options.ResizeWidth.HasValue)
            {
                photo.Filters.Add(new PhotoFilter
                {
                    FilterType = "Resize",
                    FilterValue = $"{command.Options.ResizeWidth}x{command.Options.ResizeHeight ?? command.Options.ResizeWidth}"
                });
            }

            if (!string.IsNullOrWhiteSpace(command.Options.OutputFormat))
            {
                photo.Filters.Add(new PhotoFilter
                {
                    FilterType = "Format",
                    FilterValue = command.Options.OutputFormat
                });
            }

            if (command.Options.ApplySepia)
            {
                photo.Filters.Add(new PhotoFilter
                {
                    FilterType = "Sepia",
                    FilterValue = "true"
                });
            }

            if (command.Options.BlurAmount.HasValue)
            {
                photo.Filters.Add(new PhotoFilter
                {
                    FilterType = "Blur",
                    FilterValue = command.Options.BlurAmount.Value.ToString()
                });
            }

            // Hashtags
            foreach (var tag in SplitTags(command.HashtagsRaw))
            {
                var hashtag = await _hashtags.GetOrCreateAsync(tag, cancellationToken);
                photo.Hashtags.Add(new PhotoHashtag { Hashtag = hashtag });
            }

            _photos.Add(photo);
            await _photos.SaveChangesAsync(cancellationToken);

            await _events.PublishAsync(new PhotoUploadedEvent(photo.Id, command.User.Id), cancellationToken);

            return new UploadPhotoResult { PhotoId = photo.Id };
        }

        private static IEnumerable<string> SplitTags(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                yield break;

            foreach (var t in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var normalized = t.ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(normalized))
                    yield return normalized;
            }
        }
    }
}
