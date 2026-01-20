using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;

namespace PhotoGallery.Infrastructure.ImageProcessing.Templates
{
    public sealed class StorageImageTransformTemplate : ImageTransformTemplate
    {
        public StorageImageTransformTemplate(IImageProcessorFactory processorFactory) : base(processorFactory)
        {
        }

        protected override bool ShouldCreateThumbnail(ImageProcessingOptionsDto options) => true;
    }
}
