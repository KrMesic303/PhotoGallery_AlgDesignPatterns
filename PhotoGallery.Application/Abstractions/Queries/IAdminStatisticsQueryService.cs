using PhotoGallery.Application.DTOs.Admin;

namespace PhotoGallery.Application.Abstractions.Queries
{
    public interface IAdminStatisticsQueryService
    {
        Task<AdminStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);
    }
}
