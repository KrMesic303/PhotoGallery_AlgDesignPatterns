using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Web.Controllers
{
    [Authorize]
    public class PhotosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPhotoStorageService _storage;
        private readonly IPhotoUploadPolicy _uploadPolicy;
        private readonly UserManager<ApplicationUser> _userManager;

        public PhotosController(ApplicationDbContext context, IPhotoStorageService storage, IPhotoUploadPolicy uploadPolicy, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _storage = storage;
            _uploadPolicy = uploadPolicy;
            _userManager = userManager;
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

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Upload policy
            var policyResult = await _uploadPolicy.CanUploadAsync(user, file.Length);
            if (!policyResult.IsAllowed)
            {
                ModelState.AddModelError("", policyResult.ErrorMessage!);
                return View();
            }

            // Save file
            var storageKey = await _storage.SaveAsync(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType);

            var photo = new Photo
            {
                StorageKey = storageKey,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                SizeInBytes = file.Length,
                Description = description ?? "",
                UserId = user.Id
            };

            foreach (var tag in (hashtags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var normalized = tag.ToLowerInvariant();
                var hashtag = await _context.Hashtags.FirstOrDefaultAsync(h => h.Value == normalized)
                              ?? new Hashtag { Value = normalized };

                photo.Hashtags.Add(new PhotoHashtag { Hashtag = hashtag });
            }

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
