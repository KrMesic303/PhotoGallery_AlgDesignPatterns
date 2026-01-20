using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs.PhotoGallery.Application.DTOs;
using PhotoGallery.Application.UseCases.Photos.Delete;
using PhotoGallery.Application.UseCases.Photos.Download;
using PhotoGallery.Application.UseCases.Photos.Edit;
using PhotoGallery.Application.UseCases.Photos.EditRead;
using PhotoGallery.Application.UseCases.Photos.Upload;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Web.ViewModels;
using System.Security.Claims;

namespace PhotoGallery.Web.Controllers
{
    [Authorize]
    public class PhotosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUploadQuotaService _quotaService;

        private readonly IUploadPhotoHandler _uploadHandler;
        private readonly IEditPhotoMetadataHandler _editHandler;
        private readonly IDeletePhotoHandler _deleteHandler;
        private readonly IDownloadPhotoHandler _downloadHandler;
        private readonly IGetEditPhotoHandler _getEditHandler;

        public PhotosController(
            UserManager<ApplicationUser> userManager,
            IUploadQuotaService quotaService,
            IUploadPhotoHandler uploadHandler,
            IEditPhotoMetadataHandler editHandler,
            IDeletePhotoHandler deleteHandler,
            IDownloadPhotoHandler downloadHandler,
            IGetEditPhotoHandler getEditHandler)
        {
            _userManager = userManager;
            _quotaService = quotaService;
            _uploadHandler = uploadHandler;
            _editHandler = editHandler;
            _deleteHandler = deleteHandler;
            _downloadHandler = downloadHandler;
            _getEditHandler = getEditHandler;
        }

        // GET: /Photos/Upload
        [HttpGet]
        public async Task<IActionResult> Upload(CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            ViewBag.Quota = await _quotaService.GetQuotaAsync(user, ct);
            return View();
        }

        // POST: /Photos/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, string description, string hashtags, int? resize, string format, bool applySepia, float? blurAmount, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "File is required.");
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var cmd = new UploadPhotoCommand
            {
                User = user,
                FileStream = file.OpenReadStream(),
                OriginalFileName = file.FileName,
                OriginalContentType = file.ContentType,
                FileSizeBytes = file.Length,
                Description = description ?? string.Empty,
                HashtagsRaw = hashtags ?? string.Empty,
                Options = new ImageProcessingOptionsDto
                {
                    ResizeHeight = resize,
                    ResizeWidth = resize,
                    OutputFormat = format,
                    ApplySepia = applySepia,
                    BlurAmount = blurAmount
                }
            };

            try
            {
                await _uploadHandler.HandleAsync(cmd, ct);
                return RedirectToAction("Index", "Gallery");
            }
            catch (InvalidOperationException ex)
            {
                // For errors from handler
                ModelState.AddModelError("", ex.Message);
                ViewBag.Quota = await _quotaService.GetQuotaAsync(user, ct);
                return View();
            }
        }

        // GET: /Photos/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var query = new GetEditPhotoQuery
            {
                PhotoId = id,
                RequestUserId = userId,
                IsAdmin = User.IsInRole("Administrator")
            };

            try
            {
                var result = await _getEditHandler.HandleAsync(query, ct);

                var model = new EditPhotoViewModel
                {
                    PhotoId = result.PhotoId,
                    Description = result.Description,
                    Hashtags = result.HashtagsCsv
                };

                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // POST: /Photos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPhotoViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var cmd = new EditPhotoMetadataCommand
            {
                PhotoId = model.PhotoId,
                UserId = userId,
                IsAdmin = User.IsInRole("Administrator"),
                Description = model.Description ?? string.Empty,
                HashtagsRaw = model.Hashtags ?? string.Empty
            };

            try
            {
                await _editHandler.HandleAsync(cmd, ct);
                return RedirectToAction("Details", "Gallery", new { id = model.PhotoId });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: /Photos/Download/{id}
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Download(int id)
        {
            return View(new DownloadPhotoViewModel { PhotoId = id });
        }

        // POST: /Photos/Download
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Download(DownloadPhotoViewModel model, CancellationToken ct)
        {
            var userIdOrAnonymous = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)!
                : "ANONYMOUS";

            var query = new DownloadPhotoQuery
            {
                PhotoId = model.PhotoId,
                DownloadOriginal = model.DownloadOriginal,
                RequestUserIdOrAnonymous = userIdOrAnonymous,
                Options = new ImageProcessingOptionsDto
                {
                    ResizeHeight = model.Resize,
                    ResizeWidth = model.Resize,
                    OutputFormat = model.Format,
                    ApplySepia = model.ApplySepia,
                    BlurAmount = model.BlurAmount
                }
            };

            try
            {
                var result = await _downloadHandler.HandleAsync(query, ct);
                return File(result.Stream, result.ContentType, result.FileName);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var cmd = new DeletePhotoCommand
            {
                PhotoId = id,
                UserId = userId,
                IsAdmin = true
            };

            try
            {
                await _deleteHandler.HandleAsync(cmd, ct);
                return RedirectToAction("Index", "Gallery");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}