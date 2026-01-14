using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions.Queries;
using PhotoGallery.Application.DTOs.Admin;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Infrastructure.Queries.Admin
{
    public sealed class AdminStatisticsQueryService : IAdminStatisticsQueryService
    {
        private readonly ApplicationDbContext _context;

        public AdminStatisticsQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var last7Days = now.AddDays(-7);

            var usersCount = await _context.Users.CountAsync(cancellationToken);
            var photosCount = await _context.Photos.CountAsync(cancellationToken);
            var storageUsed = await _context.Photos.SumAsync(p => p.SizeInBytes, cancellationToken);

            var totalUploads = await _context.AuditLogs
                .CountAsync(l => l.Action == "UPLOAD_PHOTO", cancellationToken);

            var uploadsLast7Days = await _context.AuditLogs
                .CountAsync(l => l.Action == "UPLOAD_PHOTO" && l.CreatedAtUtc >= last7Days, cancellationToken);

            var largestPhotoSize = await _context.Photos
                .MaxAsync(p => (long?)p.SizeInBytes, cancellationToken) ?? 0;

            var averagePhotoSize = await _context.Photos.AnyAsync(cancellationToken)
                ? await _context.Photos.AverageAsync(p => p.SizeInBytes, cancellationToken)
                : 0;

            var downloadGroups = await _context.AuditLogs
                .AsNoTracking()
                .Where(l => l.Action == "DOWNLOAD_PHOTO")
                .GroupBy(l => l.EntityId)
                .Select(g => new { EntityId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync(cancellationToken);

            var photoIds = downloadGroups
                .Select(x => int.TryParse(x.EntityId, out var id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            var photoDescriptions = await _context.Photos
                .AsNoTracking()
                .Where(p => photoIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Description })
                .ToListAsync(cancellationToken);

            var topDownloaded = downloadGroups.Select(g =>
            {
                var parsed = int.TryParse(g.EntityId, out var id);
                var description = parsed
                    ? photoDescriptions.FirstOrDefault(p => p.Id == id)?.Description ?? ""
                    : "";

                return new PhotoDownloadStatDto
                {
                    PhotoId = parsed ? id : 0,
                    DownloadCount = g.Count,
                    Description = description
                };
            }).ToList();

            var storagePerUser = await _context.Photos
                .AsNoTracking()
                .GroupBy(p => p.User.Email)
                .Select(g => new UserStorageStatDto
                {
                    UserEmail = g.Key!,
                    StorageUsed = g.Sum(p => p.SizeInBytes)
                })
                .OrderByDescending(x => x.StorageUsed)
                .ToListAsync(cancellationToken);

            return new AdminStatisticsDto
            {
                Users = usersCount,
                Photos = photosCount,
                StorageUsed = storageUsed,
                TotalUploads = totalUploads,
                UploadsLast7Days = uploadsLast7Days,
                LargestPhotoSize = largestPhotoSize,
                AveragePhotoSize = averagePhotoSize,
                TotalDownloads = downloadGroups.Sum(x => x.Count),
                TopDownloadedPhotos = topDownloaded,
                StoragePerUser = storagePerUser
            };
        }
    }
}
