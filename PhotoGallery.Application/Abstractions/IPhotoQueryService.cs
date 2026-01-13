using PhotoGallery.Application.DTOs;

namespace PhotoGallery.Application.Abstractions
{
    public interface IPhotoQueryService
    {
        Task<PagedResult<PhotoListItemDto>> GetLatestPagedAsync(int page, int pageSize);

        Task<List<PhotoListItemDto>> GetLatestAsync(int count);
        Task<PhotoDetailsDto?> GetDetailsAsync(int photoId);
        Task<List<PhotoListItemDto>> SearchAsync(PhotoSearchCriteriaDto criteria);

        Task<List<PhotoListItemDto>> QuickSearchAsync(string term);

    }
}
