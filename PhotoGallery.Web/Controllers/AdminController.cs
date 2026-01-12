using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;
using PhotoGallery.Web.ViewModels;

namespace PhotoGallery.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPhotoQueryService _photos;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IPhotoQueryService photos)
        {
            _context = context;
            _userManager = userManager;
            _photos = photos;
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
