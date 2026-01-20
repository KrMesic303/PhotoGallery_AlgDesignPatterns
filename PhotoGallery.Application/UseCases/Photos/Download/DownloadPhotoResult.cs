namespace PhotoGallery.Application.UseCases.Photos.Download
{
    public sealed class DownloadPhotoResult
    {
        public required Stream Stream { get; init; }
        public required string ContentType { get; init; }
        public required string FileName { get; init; }
    }
}
