using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Web.Controllers
{
    /// <summary>
    /// PATTERN: Command pattern
    /// SOLID: SRP, DI
    /// </summary>
    public class FilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPhotoStorageService _storage;

        public FilesController(
            ApplicationDbContext context,
            IPhotoStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        [HttpGet]
        public async Task<IActionResult> Photo(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null)
                return NotFound();

            var stream = await _storage.GetAsync(photo.StorageKey);
            return File(stream, photo.ContentType);
        }

        [HttpGet]
        public async Task<IActionResult> Thumbnail(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null || string.IsNullOrEmpty(photo.ThumbnailStorageKey))
                return NotFound();

            var stream = await _storage.GetAsync(photo.ThumbnailStorageKey);
            return File(stream, "image/jpeg");
        }
    }
}
