namespace PhotoGallery.Application.UseCases.Photos.Download
{
    public interface IDownloadPhotoHandler
    {
        Task<DownloadPhotoResult> HandleAsync(DownloadPhotoQuery query, CancellationToken cancellationToken = default);
    }
}
