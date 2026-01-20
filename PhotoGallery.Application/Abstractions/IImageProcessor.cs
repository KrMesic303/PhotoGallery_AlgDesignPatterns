using SixLabors.ImageSharp;

namespace PhotoGallery.Application.Abstractions
{
    /// <summary>
    /// PATTERN: Strategy
    /// SOLID: Open/Closed, SRP
    /// </summary>
    public interface IImageProcessor
    {
        Image Process(Image image);
    }
}
