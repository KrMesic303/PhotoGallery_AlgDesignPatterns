using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.Abstractions.Queries
{
    public interface IAdminPhotoQueryService
    {
        Task<List<Photo>> GetPhotosWithUsersAsync(CancellationToken cancellationToken = default);
        Task<List<Photo>> GetPhotosByIdsAsync(int[] photoIds, CancellationToken cancellationToken = default);
    }
}
