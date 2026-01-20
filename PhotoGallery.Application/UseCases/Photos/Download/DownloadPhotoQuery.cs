using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;

namespace PhotoGallery.Application.UseCases.Photos.Download
{
    public sealed class DownloadPhotoQuery
    {
        public required int PhotoId { get; init; }
        public required bool DownloadOriginal { get; init; }

        // If DownloadOriginal = false
        public required ImageProcessingOptionsDto Options { get; init; }

        public required string RequestUserIdOrAnonymous { get; init; }
    }
}
