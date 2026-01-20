using PhotoGallery.Application.Abstractions;
using SixLabors.ImageSharp;

namespace PhotoGallery.Infrastructure.ImageProcessing
{
    /// <summary>
    /// PATTERN: Chain of Responsibility
    /// </summary>
    public class ImageProcessingPipeline
    {
        private readonly IList<IImageProcessor> _processors = new List<IImageProcessor>();

        public void AddProcessors(IEnumerable<IImageProcessor> processors)
        {
            foreach (var p in processors)
                AddProcessor(p);
        }

        public void AddProcessor(IImageProcessor processor)
        {
            _processors.Add(processor);
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
