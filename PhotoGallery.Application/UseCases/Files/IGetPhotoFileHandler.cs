namespace PhotoGallery.Application.UseCases.Files
{
    public interface IGetPhotoFileHandler
    {
        Task<GetPhotoFileResult> HandleAsync(GetPhotoFileQuery query, CancellationToken cancellationToken = default);
    }
}
