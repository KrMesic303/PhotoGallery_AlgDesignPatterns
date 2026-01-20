namespace PhotoGallery.Application.UseCases.Photos.Delete
{
    public interface IDeletePhotoHandler
    {
        Task HandleAsync(DeletePhotoCommand command, CancellationToken cancellationToken = default);
    }
}
