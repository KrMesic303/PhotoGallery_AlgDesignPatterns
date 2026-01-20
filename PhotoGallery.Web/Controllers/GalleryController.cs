using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs;

namespace PhotoGallery.Web.Controllers
{
    /// <summary>
    /// PATTERN: Command
    /// </summary>
    public class GalleryController(IPhotoQueryService photos) : Controller
    {
        private readonly IPhotoQueryService _photos = photos;

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
        {
            const int pageSize = 10;
            if (page < 1) page = 1;

            var result = await _photos.GetLatestPagedAsync(page, pageSize);
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct = default)
        {
            var photo = await _photos.GetDetailsAsync(id);
            if (photo == null)
                return NotFound();

            return View(photo);
        }

        [HttpGet]
        public IActionResult QuickSearch()
        {
            return PartialView("_QuickSearchModal");
        }

        [HttpPost]
        public async Task<IActionResult> QuickSearch(string query, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return PartialView("_SearchResults", new List<PhotoListItemDto>());

            var results = await _photos.QuickSearchAsync(query.Trim());
            return PartialView("_SearchResults", results);
        }
    }
}
