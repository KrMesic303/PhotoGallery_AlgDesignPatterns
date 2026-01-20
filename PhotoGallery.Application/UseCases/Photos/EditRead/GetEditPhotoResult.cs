namespace PhotoGallery.Application.UseCases.Photos.EditRead
{
    public sealed class GetEditPhotoResult
    {
        public required int PhotoId { get; init; }
        public required string Description { get; init; }
        public required string HashtagsCsv { get; init; }
    }
}
