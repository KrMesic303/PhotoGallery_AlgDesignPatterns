using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;

namespace PhotoGallery.Application.Abstractions
{
    public interface IImageProcessorFactory
    {
        IReadOnlyList<IImageProcessor> Create(ImageProcessingOptionsDto options);
    }
}
