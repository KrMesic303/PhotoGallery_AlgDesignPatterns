using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;

namespace PhotoGallery.Application.Abstractions
{
    /// <summary>
    /// PATTERN: Factory
    /// </summary>
    public interface IImageProcessorFactory
    {
        IReadOnlyList<IImageProcessor> Create(ImageProcessingOptionsDto options);
    }
}
