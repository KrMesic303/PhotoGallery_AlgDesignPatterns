namespace PhotoGallery.Application.UseCases.Admin.BulkDeletePhotos
{
    public interface IBulkDeletePhotosHandler
    {
        Task HandleAsync(BulkDeletePhotosCommand command, CancellationToken cancellationToken = default);
    }
}
