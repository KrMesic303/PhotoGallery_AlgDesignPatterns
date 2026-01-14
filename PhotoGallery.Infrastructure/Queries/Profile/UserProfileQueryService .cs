using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions.Queries;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Infrastructure.Queries.Profile
{
    public sealed class UserProfileQueryService : IUserProfileQueryService
    {
        private readonly ApplicationDbContext _context;

        public UserProfileQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<ApplicationUser?> GetUserWithPlanAsync(string userId, CancellationToken cancellationToken = default)
        {
            return _context.Users
                .AsNoTracking()
                .Include(u => u.PackagePlan)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }
    }
}
