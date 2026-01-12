using SixLabors.ImageSharp;

namespace PhotoGallery.Application.Abstractions
{
    /// <summary>
    /// Decorator pattern ( Open/Closed, SRP)
    /// </summary>
    public interface IImageProcessor
    {
        Image Process(Image image);
    }
}
