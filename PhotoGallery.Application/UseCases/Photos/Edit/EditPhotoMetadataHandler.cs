using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Repositories;
using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.UseCases.Photos.Edit
{
    public sealed class EditPhotoMetadataHandler : IEditPhotoMetadataHandler
    {
        private readonly IPhotoRepository _photos;
        private readonly IHashtagRepository _hashtags;
        private readonly IAuditLogger _audit;

        public EditPhotoMetadataHandler(IPhotoRepository photos, IHashtagRepository hashtags, IAuditLogger audit)
        {
            _photos = photos;
            _hashtags = hashtags;
            _audit = audit;
        }

        public async Task HandleAsync(EditPhotoMetadataCommand command, CancellationToken cancellationToken = default)
        {
            var photo = await _photos.GetWithHashtagsAsync(command.PhotoId, cancellationToken);
            if (photo == null)
                throw new KeyNotFoundException("Photo not found.");

            if (photo.UserId != command.UserId && !command.IsAdmin)
                throw new UnauthorizedAccessException("Forbidden.");

            photo.Description = command.Description ?? string.Empty;

            photo.Hashtags.Clear();
            foreach (var tag in SplitTags(command.HashtagsRaw))
            {
                var hashtag = await _hashtags.GetOrCreateAsync(tag, cancellationToken);
                photo.Hashtags.Add(new PhotoHashtag { Hashtag = hashtag });
            }

            await _photos.SaveChangesAsync(cancellationToken);
            await _audit.LogAsync(command.UserId, "EDIT_PHOTO_METADATA", nameof(Photo), photo.Id.ToString(), cancellationToken);
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
