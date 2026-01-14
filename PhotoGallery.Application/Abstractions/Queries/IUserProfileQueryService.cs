using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.Abstractions.Queries
{
    public interface IUserProfileQueryService
    {
        Task<ApplicationUser?> GetUserWithPlanAsync(string userId, CancellationToken cancellationToken = default);
    }
}
