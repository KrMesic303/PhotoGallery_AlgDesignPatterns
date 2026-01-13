using PhotoGallery.Application.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PhotoGallery.Infrastructure.ImageProcessing
{
    public class SepiaImageProcessor : IImageProcessor
    {
        public Image Process(Image image)
        {
            image.Mutate(ctx => ctx.Sepia());
            return image;
        }

        public string Name => "Sepia";
    }
}
