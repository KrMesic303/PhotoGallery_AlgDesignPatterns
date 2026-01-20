namespace PhotoGallery.Application.UseCases.Photos.EditRead
{
    public sealed class GetEditPhotoQuery
    {
        public required int PhotoId { get; init; }
        public required string RequestUserId { get; init; }
        public required bool IsAdmin { get; init; }
    }
}
