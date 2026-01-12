using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;
using PhotoGallery.Infrastructure.ImageProcessing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System.Security.Claims;

namespace PhotoGallery.Web.Controllers
{
    [Authorize]
    public class PhotosController : Controller
    {
        private readonly IAuditLogger _auditLogger;

        private readonly ApplicationDbContext _context;
        private readonly IPhotoStorageService _storage;
        private readonly IUploadQuotaService _quotaService;
        private readonly IPhotoUploadPolicy _uploadPolicy;
        private readonly UserManager<ApplicationUser> _userManager;

        public PhotosController(ApplicationDbContext context, IPhotoStorageService storage, IPhotoUploadPolicy uploadPolicy, UserManager<ApplicationUser> userManager, IAuditLogger auditLogger, IUploadQuotaService quotaService)
        {
            _context = context;
            _storage = storage;
            _uploadPolicy = uploadPolicy;
            _userManager = userManager;
            _auditLogger = auditLogger;
            _quotaService = quotaService;
        }

        // Command pattern ( SRP - controller invokes commands and services/classes receive commands)

        // GET: /Photos/Upload
        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            ViewBag.Quota = await _quotaService.GetQuotaAsync(user);
            return View();
        }

        // POST: /Photos/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, string description, string hashtags, int? resize, string format)
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

            // Load image
            using var image = await Image.LoadAsync(file.OpenReadStream());
            var pipeline = new ImageProcessingPipeline();

            // Resize processor
            if (resize.HasValue)
                pipeline.AddProcessor(new ResizeImageProcessor(resize.Value, resize.Value));

            // Format processor
            var formatProcessor = new FormatImageProcessor(format ?? "jpg");
            pipeline.AddProcessor(formatProcessor);

            pipeline.Execute(image);

            using var outputStream = new MemoryStream();

            switch (format?.ToLowerInvariant())
            {
                case "png":
                    await image.SaveAsync(outputStream, new PngEncoder());
                    break;
                case "bmp":
                    await image.SaveAsync(outputStream, new BmpEncoder());
                    break;
                default:
                    await image.SaveAsync(outputStream, new JpegEncoder());
                    break;
            }

            outputStream.Position = 0;
            var storedFileName = Path.GetFileNameWithoutExtension(file.FileName) + formatProcessor.GetExtension();
            var storageKey = await _storage.SaveAsync(outputStream, storedFileName, file.ContentType);

            var photo = new Photo
            {
                StorageKey = storageKey,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                SizeInBytes = outputStream.Length,
                Description = description ?? string.Empty,
                UserId = user.Id,
                UploadedAtUtc = DateTime.UtcNow
            };

            // Hashtags
            foreach (var tag in (hashtags ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var normalized = tag.ToLowerInvariant();

                var hashtag = await _context.Hashtags
                    .FirstOrDefaultAsync(h => h.Value == normalized)
                    ?? new Hashtag { Value = normalized };

                photo.Hashtags.Add(new PhotoHashtag { Hashtag = hashtag });
            }

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            // Observer pattern - audit logging
            await _auditLogger.LogAsync(user.Id, action: "UPLOAD_PHOTO", entityType: nameof(Photo), entityId: photo.Id.ToString());

            return RedirectToAction("Index", "Home");
        }
    }
}
