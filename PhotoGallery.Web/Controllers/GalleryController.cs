using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs;

namespace PhotoGallery.Web.Controllers
{
    /// <summary>
    /// PATTERN: Command pattern
    /// SOLID: SRP
    /// </summary>
    public class GalleryController : Controller
    {
        private readonly IPhotoQueryService _photos;

        public GalleryController(IPhotoQueryService photos)
        {
            _photos = photos;
        }

        public async Task<IActionResult> Index(int page = 1)
        {

            const int pageSize = 10;

            var result = await _photos.GetLatestPagedAsync(page, pageSize);

            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var photo = await _photos.GetDetailsAsync(id);
            if (photo == null) return NotFound();

            return View(photo);
        }

        [HttpGet]
        public IActionResult QuickSearch()
        {
            return PartialView("_QuickSearchModal");
        }

        [HttpPost]
        public async Task<IActionResult> QuickSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return PartialView("_SearchResults", new List<PhotoListItemDto>());

            var results = await _photos.QuickSearchAsync(query.Trim());
            return PartialView("_SearchResults", results);
        }
    }
}
