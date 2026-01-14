using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Abstractions.Repositories;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Web.Controllers
{
    /// <summary>
    /// PATTERN: Command pattern
    /// SOLID: SRP, DI
    /// </summary>
    public class FilesController : Controller
    {
        private readonly IPhotoRepository _photos;
        private readonly IPhotoStorageService _storage;

        public FilesController(
            IPhotoRepository photos,
            IPhotoStorageService storage)
        {
            _photos = photos;
            _storage = storage;
        }

        [HttpGet]
        public async Task<IActionResult> Photo(int id)
        {
            var photo = await _photos.FindAsync(id);
            if (photo == null)
                return NotFound();

            var stream = await _storage.GetAsync(photo.StorageKey);
            return File(stream, photo.ContentType);
        }

        [HttpGet]
        public async Task<IActionResult> Thumbnail(int id)
        {
            var photo = await _photos.FindAsync(id);
            if (photo == null || string.IsNullOrEmpty(photo.ThumbnailStorageKey))
                return NotFound();

            var stream = await _storage.GetAsync(photo.ThumbnailStorageKey);
            return File(stream, "image/jpeg");
        }
    }
}
