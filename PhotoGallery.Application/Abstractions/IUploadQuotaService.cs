using PhotoGallery.Application.DTOs;
using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.Abstractions
{
    public interface IUploadQuotaService
    {
        Task<UploadQuotaDto> GetQuotaAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    }
}
