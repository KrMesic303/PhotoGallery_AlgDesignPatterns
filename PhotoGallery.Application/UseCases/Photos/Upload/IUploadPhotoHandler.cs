namespace PhotoGallery.Application.UseCases.Photos.Upload
{
    public interface IUploadPhotoHandler
    {
        Task<UploadPhotoResult> HandleAsync(UploadPhotoCommand command, CancellationToken cancellationToken = default);
    }
}
