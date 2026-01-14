using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions.Queries;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Infrastructure.Queries.Admin
{
    public sealed class AdminPhotoQueryService : IAdminPhotoQueryService
    {
        private readonly ApplicationDbContext _context;

        public AdminPhotoQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<Photo>> GetPhotosWithUsersAsync(CancellationToken cancellationToken = default)
        {
            return _context.Photos
                .AsNoTracking()
                .Include(p => p.User)
                .OrderByDescending(p => p.UploadedAtUtc)
                .ToListAsync(cancellationToken);
        }

        public Task<List<Photo>> GetPhotosByIdsAsync(int[] photoIds, CancellationToken cancellationToken = default)
        {
            return _context.Photos
                .Where(p => photoIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
