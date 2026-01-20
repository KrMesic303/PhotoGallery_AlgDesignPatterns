using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace PhotoGallery.Infrastructure.ImageProcessing.Templates
{
    /// <summary>
    /// PATTERN: Template Method
    /// </summary>
    public abstract class ImageTransformTemplate
    {
        private readonly IImageProcessorFactory _processorFactory;

        protected ImageTransformTemplate(IImageProcessorFactory processorFactory)
        {
            _processorFactory = processorFactory;
        }

        // Template method
        public async Task<ImageTransformResult> TransformAsync(Stream input, string originalFileName, ImageProcessingOptionsDto options, CancellationToken ct)
        {
            using var image = await LoadAsync(input, ct);

            var processed = ApplyProcessors(image, options);

            var imageStream = new MemoryStream();
            SaveImage(processed, imageStream, options.OutputFormat);
            imageStream.Position = 0;

            var imageExtension = GetExtension(options.OutputFormat);
            var imageContentType = GetContentType(options.OutputFormat);

            if (!ShouldCreateThumbnail(options))
            {
                return new ImageTransformResult
                {
                    ImageStream = imageStream,
                    ImageContentType = imageContentType,
                    ImageExtension = imageExtension
                };
            }

            // Thumbnail
            var thumbStream = await CreateThumbnailAsync(processed, ct);

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

        // Template methods

        protected virtual Task<Image> LoadAsync(Stream input, CancellationToken ct) => Image.LoadAsync(input, ct);

        protected virtual Image ApplyProcessors(Image image, ImageProcessingOptionsDto options)
        {
            var pipeline = new ImageProcessingPipeline();
            pipeline.AddProcessors(_processorFactory.Create(options));
            return pipeline.Execute(image);
        }

        protected abstract bool ShouldCreateThumbnail(ImageProcessingOptionsDto options);

        protected virtual async Task<Stream> CreateThumbnailAsync(Image processed, CancellationToken ct)
        {
            using var thumb = processed.Clone(ctx =>
                ctx.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Crop,
                    Size = new Size(300, 300)
                }));

            var ms = new MemoryStream();
            await thumb.SaveAsJpegAsync(ms, ct);
            ms.Position = 0;
            return ms;
        }

        #region Helpers

        protected static void SaveImage(Image image, Stream output, string? format)
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

        protected static string GetContentType(string? format) =>
            (format ?? "jpg").ToLowerInvariant() switch
            {
                "png" => "image/png",
                "bmp" => "image/bmp",
                _ => "image/jpeg"
            };

        protected static string GetExtension(string? format) => "." + (format ?? "jpg").ToLowerInvariant();
        
        #endregion
    }
}
