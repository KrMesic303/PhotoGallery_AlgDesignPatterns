using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace PhotoGallery.Infrastructure.ImageProcessing
{
    public sealed class ImageSharpTransformService : IImageTransformService
    {
        private readonly IImageProcessorFactory _processorFactory;

        public ImageSharpTransformService(IImageProcessorFactory processorFactory)
        {
            _processorFactory = processorFactory;
        }

        public Task<ImageTransformResult> TransformForStorageAsync(Stream input, string originalFileName, ImageProcessingOptionsDto options, CancellationToken cancellationToken = default)
        {
            //creating thumnbain and image
            return TransformInternalAsync(input, originalFileName, options, includeThumbnail: true, cancellationToken);
        }

        public Task<ImageTransformResult> TransformForDownloadAsync(Stream input, string originalFileName, ImageProcessingOptionsDto options, CancellationToken cancellationToken = default)
        {
            //downloading final image
            return TransformInternalAsync(input, originalFileName, options, includeThumbnail: false, cancellationToken);
        }

        private async Task<ImageTransformResult> TransformInternalAsync(Stream input, string originalFileName, ImageProcessingOptionsDto options, bool includeThumbnail, CancellationToken ct)
        {
            using var originalImage = await Image.LoadAsync(input, ct);

            var pipeline = new ImageProcessingPipeline();
            pipeline.AddProcessors(_processorFactory.Create(options));

            var processedImage = pipeline.Execute(originalImage);

            var imageStream = new MemoryStream();
            SaveImage(processedImage, imageStream, options.OutputFormat);
            imageStream.Position = 0;

            var imageExtension = GetExtension(options.OutputFormat);
            var imageContentType = GetContentType(options.OutputFormat);

            if (!includeThumbnail)
            {
                return new ImageTransformResult
                {
                    ImageStream = imageStream,
                    ImageContentType = imageContentType,
                    ImageExtension = imageExtension
                };
            }

            // Create thumbnail
            using var thumbImage = processedImage.Clone(ctx =>
                ctx.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Crop,
                    Size = new Size(300, 300)
                }));

            var thumbStream = new MemoryStream();
            await thumbImage.SaveAsJpegAsync(thumbStream, ct);
            thumbStream.Position = 0;

            return new ImageTransformResult
            {
                ImageStream = imageStream,
                ImageContentType = imageContentType,
                ImageExtension = imageExtension,
                ThumbnailStream = thumbStream,
                ThumbnailContentType = "image/jpeg",
                ThumbnailExtension = ".jpg"
            };
        }

        private static void SaveImage(Image image, Stream output, string? format)
        {
            switch ((format ?? "jpg").ToLowerInvariant())
            {
                case "png":
                    image.Save(output, new PngEncoder());
                    break;
                case "bmp":
                    image.Save(output, new BmpEncoder());
                    break;
                default:
                    image.Save(output, new JpegEncoder());
                    break;
            }
        }

        private static string GetContentType(string? format) =>
            (format ?? "jpg").ToLowerInvariant() switch
            {
                "png" => "image/png",
                "bmp" => "image/bmp",
                _ => "image/jpeg"
            };

        private static string GetExtension(string? format) => "." + (format ?? "jpg").ToLowerInvariant();
    }
}
