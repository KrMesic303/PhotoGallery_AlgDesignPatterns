using PhotoGallery.Application.DTOs;

namespace PhotoGallery.Application.Abstractions
{
    public interface IPhotoQueryService
    {
        Task<List<PhotoListItemDto>> GetLatestAsync(int count);
        Task<PhotoDetailsDto?> GetDetailsAsync(int photoId);
        Task<List<PhotoListItemDto>> SearchAsync(PhotoSearchCriteriaDto criteria);
    }
}
