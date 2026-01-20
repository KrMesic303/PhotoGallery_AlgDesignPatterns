namespace PhotoGallery.Application.UseCases.Photos.Edit
{
    public interface IEditPhotoMetadataHandler
    {
        Task HandleAsync(EditPhotoMetadataCommand command, CancellationToken cancellationToken = default);
    }
}
