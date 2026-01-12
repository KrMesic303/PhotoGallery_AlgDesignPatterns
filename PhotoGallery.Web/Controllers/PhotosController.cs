using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;
using System.Security.Claims;

namespace PhotoGallery.Web.Controllers
{
    [Authorize]
    public class PhotosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPhotoStorageService _storage;

        public PhotosController(ApplicationDbContext context, IPhotoStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        [HttpGet]
        public IActionResult Upload() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, string description, string hashtags)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "File is required.");
                return View();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var storageKey = await _storage.SaveAsync(file.OpenReadStream(), file.FileName, file.ContentType);

            var photo = new Photo
            {
                StorageKey = storageKey,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                SizeInBytes = file.Length,
                Description = description ?? "",
                UserId = userId
            };

            foreach (var tag in (hashtags ?? "")
                     .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var normalized = tag.ToLowerInvariant();
                var hashtagEntity = await _context.Hashtags.FirstOrDefaultAsync(h => h.Value == normalized)
                                   ?? new Hashtag { Value = normalized };

                photo.Hashtags.Add(new PhotoHashtag { Hashtag = hashtagEntity });
            }

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
