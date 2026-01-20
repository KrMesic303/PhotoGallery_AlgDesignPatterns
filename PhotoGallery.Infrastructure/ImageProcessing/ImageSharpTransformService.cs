using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;
using PhotoGallery.Infrastructure.ImageProcessing.Templates;

namespace PhotoGallery.Infrastructure.ImageProcessing
{
    /// <summary>
    /// PATTERN: Template class
    /// </summary>
    public sealed class ImageSharpTransformService : IImageTransformService
    {
        private readonly StorageImageTransformTemplate _storageTemplate;
        private readonly DownloadImageTransformTemplate _downloadTemplate;

        public ImageSharpTransformService(StorageImageTransformTemplate storageTemplate, DownloadImageTransformTemplate downloadTemplate)
        {
            _storageTemplate = storageTemplate;
            _downloadTemplate = downloadTemplate;
        }

        public Task<ImageTransformResult> TransformForStorageAsync(Stream input, string originalFileName, ImageProcessingOptionsDto options, CancellationToken cancellationToken = default)
        {
            return _storageTemplate.TransformAsync(input, originalFileName, options, cancellationToken);
        }

        public Task<ImageTransformResult> TransformForDownloadAsync(Stream input, string originalFileName, ImageProcessingOptionsDto options, CancellationToken cancellationToken = default)
        {
            return _downloadTemplate.TransformAsync(input, originalFileName, options, cancellationToken);
        }
    }
}
