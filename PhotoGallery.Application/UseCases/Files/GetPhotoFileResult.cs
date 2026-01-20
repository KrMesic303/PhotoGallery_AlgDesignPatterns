namespace PhotoGallery.Application.UseCases.Files
{
    public sealed class GetPhotoFileResult
    {
        public required Stream Stream { get; init; }
        public required string ContentType { get; init; }
    }
}
