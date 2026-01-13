using PhotoGallery.Application.Abstractions;
using SixLabors.ImageSharp;

namespace PhotoGallery.Infrastructure.ImageProcessing
{
    // Decorator pattern
    public class ImageProcessingPipeline
    {
        private readonly IList<IImageProcessor> _processors = new List<IImageProcessor>();

        public void AddProcessor(IImageProcessor processor)
        {
            _processors.Add(processor);
        }
        public void AddProcessors(IEnumerable<IImageProcessor> processors)
        {
            foreach (var p in processors)
                AddProcessor(p);
        }

        public Image Execute(Image image)
        {
            foreach (var processor in _processors)
            {
                image = processor.Process(image);
            }
            return image;
        }
    }
}
