namespace PhotoGallery.Application.UseCases.Admin.DeletePhoto
{
    public interface IAdminDeletePhotoHandler
    {
        Task HandleAsync(AdminDeletePhotoCommand command, CancellationToken cancellationToken = default);
    }
}
