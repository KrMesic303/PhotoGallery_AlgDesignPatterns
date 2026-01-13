using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;
using PhotoGallery.Infrastructure.Logging;
using PhotoGallery.Web.ViewModels;

namespace PhotoGallery.Web.Controllers
{
    /// <summary>
    /// PATTERN: Command pattern
    /// SOLID: SRP, DI
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly IAuditLogger _auditLogger;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPhotoQueryService _photos;
        private readonly IPhotoStorageService _storage;
        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAuditLogger auditLogger, IPhotoQueryService photos, IPhotoStorageService storage)
        {
            _context = context;
            _userManager = userManager;
            _auditLogger = auditLogger;
            _photos = photos;
            _storage = storage;
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.PackagePlan)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePackage(string userId, int packageId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.PackagePlanId = packageId;
            user.PackageEffectiveFrom = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Photos()
        {
            var photos = await _context.Photos
                .Include(p => p.User)
                .OrderByDescending(p => p.UploadedAtUtc)
                .ToListAsync();

            return View(photos);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null) return NotFound();

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Photos));
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDeletePhotos(int[] photoIds)
        {
            if (photoIds == null || photoIds.Length == 0)
                return RedirectToAction(nameof(Photos));

            var photos = await _context.Photos
                .Where(p => photoIds.Contains(p.Id))
                .ToListAsync();

            foreach (var photo in photos)
            {
                await _storage.DeleteAsync(photo.StorageKey);

                if (!string.IsNullOrEmpty(photo.ThumbnailStorageKey))
                    await _storage.DeleteAsync(photo.ThumbnailStorageKey);

                _context.Photos.Remove(photo);
            }

            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync(
                userId: User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value,
                action: "BULK_DELETE_PHOTO",
                entityType: nameof(Photo),
                entityId: string.Join(",", photoIds));

            return RedirectToAction(nameof(Photos));
        }

        public async Task<IActionResult> Logs()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.CreatedAtUtc)
                .Take(500)
                .ToListAsync();

            return View(logs);
        }

        public async Task<IActionResult> Statistics()
        {
            var now = DateTime.UtcNow;
            var last7Days = now.AddDays(-7);

            // Basic stats
            var usersCount = await _context.Users.CountAsync();
            var photosCount = await _context.Photos.CountAsync();
            var storageUsed = await _context.Photos.SumAsync(p => p.SizeInBytes);

            // Upload stats
            var totalUploads = await _context.AuditLogs
                .CountAsync(l => l.Action == "UPLOAD_PHOTO");

            var uploadsLast7Days = await _context.AuditLogs
                .CountAsync(l => l.Action == "UPLOAD_PHOTO" && l.CreatedAtUtc >= last7Days);

            // Photo size stats
            var largestPhotoSize = await _context.Photos
                .MaxAsync(p => (long?)p.SizeInBytes) ?? 0;

            var averagePhotoSize = await _context.Photos.AnyAsync()
                ? await _context.Photos.AverageAsync(p => p.SizeInBytes)
                : 0;

            // Download stats
            var downloadGroups = await _context.AuditLogs
                .Where(l => l.Action == "DOWNLOAD_PHOTO")
                .GroupBy(l => l.EntityId)
                .Select(g => new
                {
                    EntityId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // Parsing IDs
            var photoIds = downloadGroups
                .Select(x => int.TryParse(x.EntityId, out var id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            var photoDescriptions = await _context.Photos
                .Where(p => photoIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Description })
                .ToListAsync();

            var topDownloadedPhotos = downloadGroups
                .Select(g =>
                {
                    var parsed = int.TryParse(g.EntityId, out var id);
                    var description = parsed
                        ? photoDescriptions.FirstOrDefault(p => p.Id == id)?.Description ?? ""
                        : "";

                    return new PhotoDownloadStat
                    {
                        PhotoId = parsed ? id : 0,
                        DownloadCount = g.Count,
                        Description = description
                    };
                })
                .ToList();

            // Storage per user
            var storagePerUser = await _context.Photos
                .GroupBy(p => p.User.Email)
                .Select(g => new UserStorageStat
                {
                    UserEmail = g.Key!,
                    StorageUsed = g.Sum(p => p.SizeInBytes)
                })
                .OrderByDescending(x => x.StorageUsed)
                .ToListAsync();

            var stats = new AdminStatisticsViewModel
            {
                Users = usersCount,
                Photos = photosCount,
                StorageUsed = storageUsed,

                TotalUploads = totalUploads,
                UploadsLast7Days = uploadsLast7Days,

                LargestPhotoSize = largestPhotoSize,
                AveragePhotoSize = averagePhotoSize,

                TotalDownloads = downloadGroups.Sum(x => x.Count),
                TopDownloadedPhotos = topDownloadedPhotos,

                StoragePerUser = storagePerUser
            };

            return View(stats);
        }


        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult Search()
        {
            return View(new PhotoSearchCriteriaDto());
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Search(PhotoSearchCriteriaDto criteria)
        {
            var results = await _photos.SearchAsync(criteria);
            return View("SearchResults", results);
        }
    }
}
