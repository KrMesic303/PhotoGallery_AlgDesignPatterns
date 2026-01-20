namespace PhotoGallery.Application.UseCases.Admin.BulkDeletePhotos
{
    public sealed class BulkDeletePhotosCommand
    {
        public required int[] PhotoIds { get; init; }
        public required string AdminUserId { get; init; }
    }
}
