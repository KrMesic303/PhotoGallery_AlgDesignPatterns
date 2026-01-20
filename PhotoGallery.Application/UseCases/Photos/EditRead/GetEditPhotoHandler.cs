using PhotoGallery.Application.Abstractions.Repositories;

namespace PhotoGallery.Application.UseCases.Photos.EditRead
{
    public sealed class GetEditPhotoHandler : IGetEditPhotoHandler
    {
        private readonly IPhotoRepository _photos;

        public GetEditPhotoHandler(IPhotoRepository photos)
        {
            _photos = photos;
        }

        public async Task<GetEditPhotoResult> HandleAsync(GetEditPhotoQuery query, CancellationToken cancellationToken = default)
        {
            var photo = await _photos.GetWithHashtagsAsync(query.PhotoId, cancellationToken);
            if (photo == null)
                throw new KeyNotFoundException("Photo not found.");

            if (photo.UserId != query.RequestUserId && !query.IsAdmin)
                throw new UnauthorizedAccessException("Forbidden.");

            var hashtagsCsv = string.Join(", ", photo.Hashtags.Select(h => h.Hashtag.Value));

            return new GetEditPhotoResult
            {
                PhotoId = photo.Id,
                Description = photo.Description,
                HashtagsCsv = hashtagsCsv
            };
        }
    }
}
