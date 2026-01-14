using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions.Queries;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Infrastructure.Queries.Admin
{
    public sealed class AuditLogQueryService : IAuditLogQueryService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<AuditLog>> GetLatestAsync(int take = 500, CancellationToken cancellationToken = default)
        {
            if (take <= 0) take = 500;

            return _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(l => l.CreatedAtUtc)
                .Take(take)
                .ToListAsync(cancellationToken);
        }
    }
}
