using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;
using PhotoGallery.Infrastructure.ImageProcessing;
using PhotoGallery.Web.ViewModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
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
                ViewBag.Quota = await _quotaService.GetQuotaAsync(user);
                return View();
            }

            // Load originalImage
            using var originalImage = await Image.LoadAsync(file.OpenReadStream());
            var pipeline = new ImageProcessingPipeline();

            // Resize processor
            if (resize.HasValue)
                pipeline.AddProcessor(new ResizeImageProcessor(resize.Value, resize.Value));

            // Format processor
            var formatProcessor = new FormatImageProcessor(format ?? "jpg");
            pipeline.AddProcessor(formatProcessor);

            var processedImage = pipeline.Execute(originalImage);


            using var imageStream = new MemoryStream();
            SaveImage(processedImage, imageStream, formatProcessor);
            imageStream.Position = 0;

            var storageKey = await _storage.SaveAsync(imageStream, Path.GetFileNameWithoutExtension(file.FileName) + formatProcessor.GetExtension(), file.ContentType);

            // Thumnails creation
            using var thumbImage = processedImage.Clone(ctx =>
                ctx.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Crop,
                    Size = new Size(300, 300)
                }));

            using var thumbStream = new MemoryStream();
            await thumbImage.SaveAsJpegAsync(thumbStream);
            thumbStream.Position = 0;

            var thumbnailKey = await _storage.SaveAsync(thumbStream, "thumb_" + file.FileName, "image/jpeg");

            // Photo entity
            var photo = new Photo
            {
                StorageKey = storageKey,
                ThumbnailStorageKey = thumbnailKey,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                SizeInBytes = imageStream.Length,
                Description = description ?? "",
                UserId = user.Id
            };

            // Handling hastags on photos
            foreach (var tag in (hashtags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var normalized = tag.ToLowerInvariant();

                var hashtagEntity = await _context.Hashtags
                    .FirstOrDefaultAsync(h => h.Value == normalized)
                    ?? new Hashtag { Value = normalized };

                photo.Hashtags.Add(new PhotoHashtag { Hashtag = hashtagEntity });
            }

            // Save to database
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            // Audit log
            await _auditLogger.LogAsync(user.Id, action: "UPLOAD_PHOTO", entityType: nameof(Photo), entityId: photo.Id.ToString());

            return RedirectToAction("Index", "Gallery");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var photo = await _context.Photos
                .Include(p => p.Hashtags)
                .ThenInclude(ph => ph.Hashtag)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (photo == null)
                return NotFound();

            // Authorized user
            if (photo.UserId != user.Id)
                return Forbid();

            var model = new EditPhotoViewModel
            {
                PhotoId = photo.Id,
                Description = photo.Description,
                Hashtags = string.Join(", ", photo.Hashtags.Select(h => h.Hashtag.Value))
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPhotoViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var photo = await _context.Photos
                .Include(p => p.Hashtags)
                .ThenInclude(ph => ph.Hashtag)
                .FirstOrDefaultAsync(p => p.Id == model.PhotoId);

            if (photo == null)
                return NotFound();

            // Authorized user
            if (photo.UserId != user.Id)
                return Forbid();

            photo.Description = model.Description;
            
            photo.Hashtags.Clear();
            foreach (var tag in (model.Hashtags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var normalized = tag.ToLowerInvariant();

                var hashtag = await _context.Hashtags
                    .FirstOrDefaultAsync(h => h.Value == normalized)
                    ?? new Hashtag { Value = normalized };

                photo.Hashtags.Add(new PhotoHashtag { Hashtag = hashtag });
            }

            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync(user.Id, action: "EDIT_PHOTO_METADATA", entityType: nameof(Photo), entityId: photo.Id.ToString());

            return RedirectToAction("Details", "Gallery", new { id = photo.Id });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null)
                return NotFound();

            var model = new DownloadPhotoViewModel
            {
                PhotoId = id
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Download(DownloadPhotoViewModel model)
        {
            var photo = await _context.Photos.FindAsync(model.PhotoId);
            if (photo == null)
                return NotFound();

            if (model.DownloadOriginal)
            {
                var originalStream = await _storage.GetAsync(photo.StorageKey);
                return File(
                    originalStream,
                    photo.ContentType,
                    photo.OriginalFileName);
            }

            using var sourceStream = await _storage.GetAsync(photo.StorageKey);
            using var image = await Image.LoadAsync(sourceStream);

            var pipeline = new ImageProcessingPipeline();

            if (model.Resize.HasValue)
            {
                pipeline.AddProcessor(new ResizeImageProcessor(model.Resize.Value, model.Resize.Value));
            }

            var formatProcessor = new FormatImageProcessor(model.Format);
            pipeline.AddProcessor(formatProcessor);

            var processedImage = pipeline.Execute(image);

            var output = new MemoryStream();
            SaveImage(processedImage, output, formatProcessor);
            output.Position = 0;

            var fileName = Path.GetFileNameWithoutExtension(photo.OriginalFileName) + formatProcessor.GetExtension();

            await _auditLogger.LogAsync(
                userId: User.Identity?.IsAuthenticated == true
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier)!
                    : "ANONYMOUS",
                action: "DOWNLOAD_PHOTO",
                entityType: nameof(Photo),
                entityId: photo.Id.ToString());


            return File(output, "application/octet-stream", fileName);
        }


        #region Helper methods

        private static void SaveImage(Image image, Stream output, FormatImageProcessor format)
        {
            switch (format.GetExtension())
            {
                case ".png":
                    image.Save(output, new PngEncoder());
                    break;
                case ".bmp":
                    image.Save(output, new BmpEncoder());
                    break;
                default:
                    image.Save(output, new JpegEncoder());
                    break;
            }
        }

        #endregion
    }
}