using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;

namespace PhotoGallery.Infrastructure.ImageProcessing
{
    public class ImageProcessorFactory : IImageProcessorFactory
    {
        public IReadOnlyList<IImageProcessor> Create(ImageProcessingOptionsDto options)
        {
            var processors = new List<IImageProcessor>();

            if (options.ResizeWidth.HasValue)
            {
                var w = options.ResizeWidth.Value;
                var h = options.ResizeHeight ?? w;

                processors.Add(new ResizeImageProcessor(w, h));
            }

            if (!string.IsNullOrWhiteSpace(options.OutputFormat))
            {
                processors.Add(new FormatImageProcessor(options.OutputFormat.Trim().ToLowerInvariant()));
            }

            return processors;
        }
    }
}
