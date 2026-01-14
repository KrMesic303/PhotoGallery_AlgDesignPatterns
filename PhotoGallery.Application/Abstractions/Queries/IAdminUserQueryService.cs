using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.Abstractions.Queries
{
    public interface IAdminUserQueryService
    {
        Task<List<ApplicationUser>> GetUsersWithPlansAsync(CancellationToken cancellationToken = default);
    }
}
