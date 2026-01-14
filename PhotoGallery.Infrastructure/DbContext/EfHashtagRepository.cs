using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions.Repositories;
using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Infrastructure.DbContext
{
    public sealed class EfHashtagRepository : IHashtagRepository
    {
        private readonly ApplicationDbContext _context;

        public EfHashtagRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Hashtag> GetOrCreateAsync(string normalizedValue, CancellationToken cancellationToken = default)
        {
            var existing = await _context.Hashtags
                .FirstOrDefaultAsync(h => h.Value == normalizedValue, cancellationToken);

            if (existing != null)
                return existing;

            var created = new Hashtag { Value = normalizedValue };
            _context.Hashtags.Add(created);

            // Note: not calling SaveChanges here; caller commits via UoW style SaveChanges once.
            return created;
        }
    }
}
