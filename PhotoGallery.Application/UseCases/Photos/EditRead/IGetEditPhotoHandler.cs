namespace PhotoGallery.Application.UseCases.Photos.EditRead
{
    public interface IGetEditPhotoHandler
    {
        Task<GetEditPhotoResult> HandleAsync(GetEditPhotoQuery query, CancellationToken cancellationToken = default);
    }
}
