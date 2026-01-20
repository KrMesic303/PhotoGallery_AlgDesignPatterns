using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;
using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.UseCases.Photos.Upload
{
    public sealed class UploadPhotoCommand
    {
        public required ApplicationUser User { get; init; }

        public required Stream FileStream { get; init; }
        public required string OriginalFileName { get; init; }
        public required string OriginalContentType { get; init; }
        public required long FileSizeBytes { get; init; }

        public string Description { get; init; } = string.Empty;
        public string HashtagsRaw { get; init; } = string.Empty;

        public required ImageProcessingOptionsDto Options { get; init; }
    }
}
