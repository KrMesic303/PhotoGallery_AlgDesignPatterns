using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Queries;
using PhotoGallery.Application.DTOs;
using PhotoGallery.Application.UseCases.Admin.BulkDeletePhotos;
using PhotoGallery.Application.UseCases.Admin.ChangePackage;
using PhotoGallery.Application.UseCases.Admin.DeletePhoto;
using PhotoGallery.Web.ViewModels;
using System.Security.Claims;

namespace PhotoGallery.Web.Controllers
{
    /// <summary>
    /// PATTERN: Command
    /// SOLID: SRP, DI
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly IAdminUserQueryService _adminUsers;
        private readonly IAdminPhotoQueryService _adminPhotos;
        private readonly IAuditLogQueryService _auditLogs;
        private readonly IAdminStatisticsQueryService _stats;
        private readonly IPhotoQueryService _photoQuery;

        private readonly IChangePackageHandler _changePackage;
        private readonly IAdminDeletePhotoHandler _deletePhoto;
        private readonly IBulkDeletePhotosHandler _bulkDelete;

        public AdminController(
            IAdminUserQueryService adminUsers,
            IAdminPhotoQueryService adminPhotos,
            IAuditLogQueryService auditLogs,
            IAdminStatisticsQueryService stats,
            IPhotoQueryService photoQuery,
            IChangePackageHandler changePackage,
            IAdminDeletePhotoHandler deletePhoto,
            IBulkDeletePhotosHandler bulkDelete)
        {
            _adminUsers = adminUsers;
            _adminPhotos = adminPhotos;
            _auditLogs = auditLogs;
            _stats = stats;
            _photoQuery = photoQuery;
            _changePackage = changePackage;
            _deletePhoto = deletePhoto;
            _bulkDelete = bulkDelete;
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
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminUserId))
                return Unauthorized();

            await _changePackage.HandleAsync(new ChangePackageCommand
            {
                TargetUserId = userId,
                PackageId = packageId,
                AdminUserId = adminUserId
            }, ct);

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
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminUserId))
                return Unauthorized();

            try
            {
                await _deletePhoto.HandleAsync(new AdminDeletePhotoCommand
                {
                    PhotoId = id,
                    AdminUserId = adminUserId
                }, ct);

                return RedirectToAction(nameof(Photos));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDeletePhotos(int[] photoIds, CancellationToken ct)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminUserId))
                return Unauthorized();

            await _bulkDelete.HandleAsync(new BulkDeletePhotosCommand
            {
                PhotoIds = photoIds ?? Array.Empty<int>(),
                AdminUserId = adminUserId
            }, ct);

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
