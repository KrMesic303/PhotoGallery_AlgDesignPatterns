using PhotoGallery.Application.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PhotoGallery.Infrastructure.ImageProcessing
{
    public class ResizeImageProcessor : IImageProcessor
    {
        private readonly int _maxWidth;
        private readonly int _maxHeight;

        public ResizeImageProcessor(int maxWidth, int maxHeight)
        {
            _maxWidth = maxWidth;
            _maxHeight = maxHeight;
        }

        public Image Process(Image image)
        {
            image.Mutate(x =>
                x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(_maxWidth, _maxHeight)
                }));

            return image;
        }
    }
}
