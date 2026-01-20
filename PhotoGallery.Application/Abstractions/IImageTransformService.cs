using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;

namespace PhotoGallery.Application.Abstractions
{
    public interface IImageTransformService
    {
        Task<ImageTransformResult> TransformForStorageAsync(Stream input, string originalFileName, ImageProcessingOptionsDto options, CancellationToken cancellationToken = default);

        Task<ImageTransformResult> TransformForDownloadAsync(Stream input, string originalFileName, ImageProcessingOptionsDto options, CancellationToken cancellationToken = default);
    }

    public sealed class ImageTransformResult : IAsyncDisposable
    {
        public required Stream ImageStream { get; init; }
        public required string ImageContentType { get; init; }
        public required string ImageExtension { get; init; }

        public Stream? ThumbnailStream { get; init; }
        public string? ThumbnailContentType { get; init; }
        public string? ThumbnailExtension { get; init; }

        public async ValueTask DisposeAsync()
        {
            await ImageStream.DisposeAsync();

            if (ThumbnailStream != null)
                await ThumbnailStream.DisposeAsync();
        }
    }
}
