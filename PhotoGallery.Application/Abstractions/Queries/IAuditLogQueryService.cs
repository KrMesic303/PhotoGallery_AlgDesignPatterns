using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.Abstractions.Queries
{
    public interface IAuditLogQueryService
    {
        Task<List<AuditLog>> GetLatestAsync(int take = 500, CancellationToken cancellationToken = default);
    }
}