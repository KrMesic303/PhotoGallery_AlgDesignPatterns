using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions.Queries;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Infrastructure.Queries.Admin
{
    public sealed class AdminUserQueryService : IAdminUserQueryService
    {
        private readonly ApplicationDbContext _context;

        public AdminUserQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<ApplicationUser>> GetUsersWithPlansAsync(CancellationToken cancellationToken = default)
        {
            return _context.Users
                .AsNoTracking()
                .Include(u => u.PackagePlan)
                .ToListAsync(cancellationToken);
        }
    }
}
