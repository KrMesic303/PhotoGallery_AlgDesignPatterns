using PhotoGallery.Application.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PhotoGallery.Infrastructure.ImageProcessing
{
    public class BlurImageProcessor : IImageProcessor
    {
        private readonly float _amount;

        public BlurImageProcessor(float amount)
        {
            _amount = amount;
        }

        public Image Process(Image image)
        {
            image.Mutate(ctx => ctx.GaussianBlur(_amount));
            return image;
        }

        public string Name => "Blur";
    }
}
