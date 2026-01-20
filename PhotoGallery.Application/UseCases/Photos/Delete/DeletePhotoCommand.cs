namespace PhotoGallery.Application.UseCases.Photos.Delete
{
    public sealed class DeletePhotoCommand
    {
        public required int PhotoId { get; init; }
        public required string UserId { get; init; }
        public required bool IsAdmin { get; init; }
    }
}
