namespace PhotoGallery.Application.UseCases.Admin.DeletePhoto
{
    public sealed class AdminDeletePhotoCommand
    {
        public required int PhotoId { get; init; }
        public required string AdminUserId { get; init; }
    }
}
