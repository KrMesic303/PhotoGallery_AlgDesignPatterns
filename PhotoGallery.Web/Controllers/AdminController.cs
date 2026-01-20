using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Queries;
using PhotoGallery.Application.Abstractions.Repositories;
using PhotoGallery.Application.DTOs;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Web.ViewModels;
using System.Security.Claims;

namespace PhotoGallery.Web.Controllers
{
    /// <summary>
    /// PATTERN: Command pattern
    /// SOLID: SRP, DI
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogger _auditLogger;
        private readonly IPhotoStorageService _storage;

        private readonly IAdminUserQueryService _adminUsers;
        private readonly IAdminPhotoQueryService _adminPhotos;
        private readonly IAuditLogQueryService _auditLogs;
        private readonly IAdminStatisticsQueryService _stats;

        private readonly IPhotoQueryService _photoQuery;
        private readonly IPhotoRepository _photoRepository;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            IAuditLogger auditLogger,
            IPhotoStorageService storage,
            IAdminUserQueryService adminUsers,
            IAdminPhotoQueryService adminPhotos,
            IAuditLogQueryService auditLogs,
            IAdminStatisticsQueryService stats,
            IPhotoQueryService photoQuery,
            IPhotoRepository photoRepository)
        {
            _userManager = userManager;
            _auditLogger = auditLogger;
            _storage = storage;
            _adminUsers = adminUsers;
            _adminPhotos = adminPhotos;
            _auditLogs = auditLogs;
            _stats = stats;
            _photoQuery = photoQuery;
            _photoRepository = photoRepository;
        }

        public async Task<IActionResult> Users(CancellationToken ct)
        {
            var users = await _adminUsers.GetUsersWithPlansAsync(ct);
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePackage(string userId, int packageId, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.PackagePlanId = packageId;
            user.PackageEffectiveFrom = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            await _auditLogger.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                action: "ADMIN_CHANGE_PACKAGE",
                entityType: nameof(ApplicationUser),
                entityId: userId,
                cancellationToken: ct);

            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Photos(CancellationToken ct)
        {
            var photos = await _adminPhotos.GetPhotosWithUsersAsync(ct);
            return View(photos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoto(int id, CancellationToken ct)
        {
            var photo = await _photoRepository.FindAsync(id, ct);
            if (photo == null) return NotFound();

            // Remove files from storage
            await _storage.DeleteAsync(photo.StorageKey, ct);
            if (!string.IsNullOrEmpty(photo.ThumbnailStorageKey))
                await _storage.DeleteAsync(photo.ThumbnailStorageKey, ct);

            _photoRepository.Remove(photo);
            await _photoRepository.SaveChangesAsync(ct);

            await _auditLogger.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                action: "ADMIN_DELETE_PHOTO",
                entityType: nameof(Photo),
                entityId: photo.Id.ToString(),
                cancellationToken: ct);

            return RedirectToAction(nameof(Photos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDeletePhotos(int[] photoIds, CancellationToken ct)
        {
            if (photoIds == null || photoIds.Length == 0)
                return RedirectToAction(nameof(Photos));

            // Using admin query service to fetch photos
            var photos = await _adminPhotos.GetPhotosByIdsAsync(photoIds, ct);

            foreach (var photo in photos)
            {
                await _storage.DeleteAsync(photo.StorageKey, ct);

                if (!string.IsNullOrEmpty(photo.ThumbnailStorageKey))
                    await _storage.DeleteAsync(photo.ThumbnailStorageKey, ct);

                _photoRepository.Remove(photo);
            }

            await _photoRepository.SaveChangesAsync(ct);

            await _auditLogger.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                action: "BULK_DELETE_PHOTO",
                entityType: nameof(Photo),
                entityId: string.Join(",", photoIds),
                cancellationToken: ct);

            return RedirectToAction(nameof(Photos));
        }

        public async Task<IActionResult> Logs(CancellationToken ct)
        {
            var logs = await _auditLogs.GetLatestAsync(take: 500, cancellationToken: ct);
            return View(logs);
        }

        public async Task<IActionResult> Statistics(CancellationToken ct)
        {
            var dto = await _stats.GetStatisticsAsync(ct);

            var model = new AdminStatisticsViewModel
            {
                Users = dto.Users,
                Photos = dto.Photos,
                StorageUsed = dto.StorageUsed,

                TotalUploads = dto.TotalUploads,
                UploadsLast7Days = dto.UploadsLast7Days,

                LargestPhotoSize = dto.LargestPhotoSize,
                AveragePhotoSize = dto.AveragePhotoSize,

                TotalDownloads = dto.TotalDownloads,
                TopDownloadedPhotos = dto.TopDownloadedPhotos.Select(x => new PhotoDownloadStat
                {
                    PhotoId = x.PhotoId,
                    DownloadCount = x.DownloadCount,
                    Description = x.Description
                }).ToList(),

                StoragePerUser = dto.StoragePerUser.Select(x => new UserStorageStat
                {
                    UserEmail = x.UserEmail,
                    StorageUsed = x.StorageUsed
                }).ToList()
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult Search()
        {
            return View(new PhotoSearchCriteriaDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(PhotoSearchCriteriaDto criteria, CancellationToken ct)
        {
            var results = await _photoQuery.SearchAsync(criteria);
            return View("SearchResults", results);
        }
    }
}
