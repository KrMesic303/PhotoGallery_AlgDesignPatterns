using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Infrastructure.Logging
{
    /// <summary>
    /// PATTERN: Observer - Decorator
    /// SOLID: Open/Closed principle
    /// </summary>
    public class AuditLogger(ApplicationDbContext context) : IAuditLogger
    {
        private readonly ApplicationDbContext _context = context;

        public async Task LogAsync(string userId, string action, string? entityType = null, string? entityId = null, CancellationToken cancellationToken = default)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
