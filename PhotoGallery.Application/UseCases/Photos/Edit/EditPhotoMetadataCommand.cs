namespace PhotoGallery.Application.UseCases.Photos.Edit
{
    public sealed class EditPhotoMetadataCommand
    {
        public required int PhotoId { get; init; }
        public required string UserId { get; init; }
        public required bool IsAdmin { get; init; }

        public string Description { get; init; } = string.Empty;
        public string HashtagsRaw { get; init; } = string.Empty;
    }
}
