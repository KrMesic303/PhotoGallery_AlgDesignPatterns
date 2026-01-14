using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions.Repositories;
using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Infrastructure.DbContext
{
    public sealed class EfPhotoRepository : IPhotoRepository
    {
        private readonly ApplicationDbContext _context;

        public EfPhotoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Photo?> FindAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Photos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public Task<Photo?> GetWithHashtagsAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Photos
                .Include(p => p.Hashtags)
                    .ThenInclude(ph => ph.Hashtag)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public void Add(Photo photo) => _context.Photos.Add(photo);

        public void Remove(Photo photo) => _context.Photos.Remove(photo);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);
    }
}
