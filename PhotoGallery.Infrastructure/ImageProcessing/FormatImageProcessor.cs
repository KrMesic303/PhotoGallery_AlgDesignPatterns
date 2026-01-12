using PhotoGallery.Application.Abstractions;
using SixLabors.ImageSharp;

namespace PhotoGallery.Infrastructure.ImageProcessing
{
    public class FormatImageProcessor : IImageProcessor
    {
        private readonly string _format;

        public FormatImageProcessor(string format)
        {
            _format = format.ToLowerInvariant();
        }

        public Image Process(Image image) => image;

        public string GetExtension()
            => _format switch
            {
                "png" => ".png",
                "bmp" => ".bmp",
                _ => ".jpg"
            };
    }
}