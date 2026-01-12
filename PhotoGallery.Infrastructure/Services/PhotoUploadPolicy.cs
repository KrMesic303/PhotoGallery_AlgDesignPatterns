using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Infrastructure.Services
{
    public class PhotoUploadPolicy : IPhotoUploadPolicy
    {
        private readonly ApplicationDbContext _context;

        public PhotoUploadPolicy(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UploadPolicyResult> CanUploadAsync(ApplicationUser user, long fileSizeBytes, CancellationToken cancellationToken = default)
        {
            var package = await _context.PackagePlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == user.PackagePlanId, cancellationToken);

            if (package == null)
                return UploadPolicyResult.Deny("User package not found.");

            if (fileSizeBytes > package.MaxPhotoSizeBytes)
            {
                return UploadPolicyResult.Deny(
                    $"File exceeds maximum allowed size ({package.MaxPhotoSizeBytes / 1024 / 1024} MB).");
            }

            var todayUtc = DateTime.UtcNow.Date;

            var todayUploads = await _context.Photos.CountAsync(p => p.UserId == user.Id && p.UploadedAtUtc >= todayUtc, cancellationToken);

            if (todayUploads >= package.DailyUploadLimit)
            {
                return UploadPolicyResult.Deny(
                    $"Daily upload limit reached ({package.DailyUploadLimit} photos).");
            }

            var usedStorage = await _context.Photos
                .Where(p => p.UserId == user.Id)
                .SumAsync(p => p.SizeInBytes, cancellationToken);

            if (usedStorage + fileSizeBytes > package.MaxStorageBytes)
            {
                return UploadPolicyResult.Deny("Total storage limit exceeded for your package.");
            }

            return UploadPolicyResult.Allow();
        }
    }
}
