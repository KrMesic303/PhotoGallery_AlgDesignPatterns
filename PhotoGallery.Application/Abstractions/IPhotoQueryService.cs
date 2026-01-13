using PhotoGallery.Application.DTOs;

namespace PhotoGallery.Application.Abstractions
{
    /// <summary>
    /// Repository abstraction separates queries from controllers, EF also uses repositories in behind with DbSet<...>
    /// SOLID: ISP - gallery controller is dependant only on querying, not on uploading or deleting
    /// </summary>
    public interface IPhotoQueryService
    {
        Task<PagedResult<PhotoListItemDto>> GetLatestPagedAsync(int page, int pageSize);

        Task<List<PhotoListItemDto>> GetLatestAsync(int count);
        Task<PhotoDetailsDto?> GetDetailsAsync(int photoId);
        Task<List<PhotoListItemDto>> SearchAsync(PhotoSearchCriteriaDto criteria);

        Task<List<PhotoListItemDto>> QuickSearchAsync(string term);

    }
}
