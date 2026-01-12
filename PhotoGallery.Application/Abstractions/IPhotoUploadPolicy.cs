using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.Abstractions
{
    public interface IPhotoUploadPolicy
    {
        Task<UploadPolicyResult> CanUploadAsync(ApplicationUser user, long fileSizeBytes, CancellationToken cancellationToken = default);
    }
}
