namespace PhotoGallery.Application.UseCases.Files
{
    public sealed class GetPhotoFileQuery
    {
        public required int PhotoId { get; init; }
        public required bool IsThumbnail { get; init; }
    }
}
