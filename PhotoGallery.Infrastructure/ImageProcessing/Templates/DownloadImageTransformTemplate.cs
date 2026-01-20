using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;

namespace PhotoGallery.Infrastructure.ImageProcessing.Templates
{
    public sealed class DownloadImageTransformTemplate(IImageProcessorFactory processorFactory) : ImageTransformTemplate(processorFactory)
    {
        protected override bool ShouldCreateThumbnail(ImageProcessingOptionsDto options) => false;
    }
}
