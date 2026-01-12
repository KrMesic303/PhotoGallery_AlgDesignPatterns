using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs;
using PhotoGallery.Infrastructure.DbContext;
using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Infrastructure.Services
{
    public class UploadQuotaService : IUploadQuotaService
    {
        private readonly ApplicationDbContext _context;

        public UploadQuotaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UploadQuotaDto> GetQuotaAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            var package = await _context.PackagePlans
                .AsNoTracking()
                .FirstAsync(p => p.Id == user.PackagePlanId, cancellationToken);

            var today = DateTime.UtcNow.Date;

            var uploadsToday = await _context.Photos.CountAsync(
                p => p.UserId == user.Id && p.UploadedAtUtc >= today,
                cancellationToken);

            var usedStorage = await _context.Photos
                .Where(p => p.UserId == user.Id)
                .SumAsync(p => p.SizeInBytes, cancellationToken);

            return new UploadQuotaDto
            {
                RemainingUploadsToday = Math.Max(0, package.DailyUploadLimit - uploadsToday),
                RemainingStorageBytes = Math.Max(0, package.MaxStorageBytes - usedStorage),
                MaxPhotoSizeBytes = package.MaxPhotoSizeBytes
            };
        }
    }
}
