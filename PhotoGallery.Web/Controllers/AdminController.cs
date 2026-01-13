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
            var stats = new AdminStatisticsViewModel
            {
                Users = await _context.Users.CountAsync(),
                Photos = await _context.Photos.CountAsync(),
                StorageUsed = await _context.Photos.SumAsync(p => p.SizeInBytes)
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
