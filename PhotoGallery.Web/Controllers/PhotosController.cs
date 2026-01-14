using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Repositories;
using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;
using PhotoGallery.Domain.Entities;
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
    /// <summary>
    /// SOLID: DIP, LSP - We can change IStorageService, DIP - Controller doesn't depent on low-level modules, its implementing abstractions
    /// </summary>
    [Authorize]
    public class PhotosController : Controller
    {
        private readonly IAuditLogger _auditLogger;

        private readonly IPhotoRepository _photos;
        private readonly IHashtagRepository _hashtags;
        private readonly IPhotoStorageService _storage;
        private readonly IUploadQuotaService _quotaService;
        private readonly IPhotoUploadPolicy _uploadPolicy;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageProcessorFactory _processorFactory;


        public PhotosController(
            IPhotoRepository photos, 
            IHashtagRepository hashtags, 
            IPhotoStorageService storage, 
            IPhotoUploadPolicy uploadPolicy, 
            UserManager<ApplicationUser> userManager, 
            IAuditLogger auditLogger, 
            IUploadQuotaService quotaService, 
            IImageProcessorFactory processorFactory )
        {
            _photos = photos;
            _hashtags = hashtags;
            _storage = storage;
            _uploadPolicy = uploadPolicy;
            _userManager = userManager;
            _auditLogger = auditLogger;
            _quotaService = quotaService;
            _processorFactory = processorFactory;
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
        public async Task<IActionResult> Upload(
            IFormFile file,
            string description, 
            string hashtags, 
            int? resize, 
            string format, 
            bool applySepia, 
            float? blurAmount)
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

            var options = new ImageProcessingOptionsDto
            {
                ResizeHeight = resize,
                ResizeWidth = resize,
                OutputFormat = format,
                ApplySepia = applySepia,
                BlurAmount = blurAmount
            };

            var pipeline = new ImageProcessingPipeline();
            pipeline.AddProcessors(_processorFactory.Create(options));

            var processedImage = pipeline.Execute(originalImage);

            using var imageStream = new MemoryStream();
            SaveImage(processedImage, imageStream, options.OutputFormat);
            imageStream.Position = 0;

            var extension = GetExtension(options.OutputFormat);
            var contentType = GetContentType(options.OutputFormat);

            var storageKey = await _storage.SaveAsync(imageStream, Path.GetFileNameWithoutExtension(file.FileName) + extension, contentType);

            // Thumnail
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

            if (options.ResizeWidth.HasValue)
            {
                photo.Filters.Add(new PhotoFilter
                {
                    FilterType = "Resize",
                    FilterValue = $"{options.ResizeWidth}x{options.ResizeHeight}"
                });
            }

            if (!string.IsNullOrWhiteSpace(options.OutputFormat))
            {
                photo.Filters.Add(new PhotoFilter
                {
                    FilterType = "Format",
                    FilterValue = options.OutputFormat
                });
            }

            if (options.ApplySepia)
            {
                photo.Filters.Add(new PhotoFilter
                {
                    FilterType = "Sepia",
                    FilterValue = "true"
                });
            }

            if (options.BlurAmount.HasValue)
            {
                photo.Filters.Add(new PhotoFilter
                {
                    FilterType = "Blur",
                    FilterValue = options.BlurAmount.Value.ToString()
                });
            }

            // Handling hastags on photos
            foreach (var tag in (hashtags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var normalized = tag.ToLowerInvariant();
                var hashtagEntity = await _hashtags.GetOrCreateAsync(normalized);

                photo.Hashtags.Add(new PhotoHashtag { Hashtag = hashtagEntity });
            }

            // DB save
            _photos.Add(photo);
            await _photos.SaveChangesAsync();

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

            var photo = await _photos.GetWithHashtagsAsync(id);

            if (photo == null)
                return NotFound();

            // Authorized user
            if (photo.UserId != user.Id && !User.IsInRole("Administrator"))
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

            var photo = await _photos.GetWithHashtagsAsync(model.PhotoId);

            if (photo == null)
                return NotFound();

            // Authorized user
            if (photo.UserId != user.Id && !User.IsInRole("Administrator"))
                return Forbid();

            photo.Description = model.Description;

            photo.Hashtags.Clear();

            foreach (var tag in (model.Hashtags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var normalized = tag.ToLowerInvariant();
                var hashtag = await _hashtags.GetOrCreateAsync(normalized);

                photo.Hashtags.Add(new PhotoHashtag { Hashtag = hashtag });
            }

            await _photos.SaveChangesAsync();

            await _auditLogger.LogAsync(user.Id, action: "EDIT_PHOTO_METADATA", entityType: nameof(Photo), entityId: photo.Id.ToString());

            return RedirectToAction("Details", "Gallery", new { id = photo.Id });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var photo = await _photos.FindAsync(id);
            if (photo == null)
                return NotFound();

            return View(new DownloadPhotoViewModel
            {
                PhotoId = id
            });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Download(DownloadPhotoViewModel model)
        {
            var photo = await _photos.FindAsync(model.PhotoId);
            if (photo == null)
                return NotFound();

            if (model.DownloadOriginal)
            {
                var originalStream = await _storage.GetAsync(photo.StorageKey);
                return File(originalStream, photo.ContentType, photo.OriginalFileName);
            }

            using var sourceStream = await _storage.GetAsync(photo.StorageKey);
            using var image = await Image.LoadAsync(sourceStream);

            var options = new ImageProcessingOptionsDto
            {
                ResizeHeight = model.Resize,
                ResizeWidth = model.Resize,
                OutputFormat = model.Format,
                ApplySepia = model.ApplySepia,
                BlurAmount = model.BlurAmount
            };

            var pipeline = new ImageProcessingPipeline();
            // Abstract Factory Pattern
            pipeline.AddProcessors(_processorFactory.Create(options));

            var processedImage = pipeline.Execute(image);

            var output = new MemoryStream();
            SaveImage(processedImage, output, options.OutputFormat);
            output.Position = 0;

            // Download
            var extension = GetExtension(options.OutputFormat);
            var fileName = Path.GetFileNameWithoutExtension(photo.OriginalFileName) + extension;
            var contentType = GetContentType(options.OutputFormat);

            await _auditLogger.LogAsync(
                userId: User.Identity?.IsAuthenticated == true
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier)!
                    : "ANONYMOUS",
                action: "DOWNLOAD_PHOTO",
                entityType: nameof(Photo),
                entityId: photo.Id.ToString());

            return File(output, contentType, fileName);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var photo = await _photos.FindAsync(id);
            if (photo == null)
                return NotFound();

            // Remove files from storage
            await _storage.DeleteAsync(photo.StorageKey);

            if (!string.IsNullOrEmpty(photo.ThumbnailStorageKey))
                await _storage.DeleteAsync(photo.ThumbnailStorageKey);

            _photos.Remove(photo);
            await _photos.SaveChangesAsync();

            await _auditLogger.LogAsync(
                userId: User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                action: "DELETE_PHOTO",
                entityType: nameof(Photo),
                entityId: photo.Id.ToString());

            return RedirectToAction("Index", "Gallery");
        }


        #region Helper methods

        private static void SaveImage(Image image, Stream output, string? format)
        {
            switch ((format ?? "jpg").ToLowerInvariant())
            {
                case "png":
                    image.Save(output, new PngEncoder());
                    break;
                case "bmp":
                    image.Save(output, new BmpEncoder());
                    break;
                default:
                    image.Save(output, new JpegEncoder());
                    break;
            }
        }

        private static string GetContentType(string? format) =>
            (format ?? "jpg").ToLowerInvariant() switch
            {
                "png" => "image/png",
                "bmp" => "image/bmp",
                _ => "image/jpeg"
            };

        private static string GetExtension(string? format) => "." + (format ?? "jpg").ToLowerInvariant();

        #endregion
    }
}