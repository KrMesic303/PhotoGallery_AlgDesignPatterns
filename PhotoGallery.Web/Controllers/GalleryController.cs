using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs;

namespace PhotoGallery.Web.Controllers
{
    public class GalleryController : Controller
    {
        private readonly IPhotoQueryService _photos;

        public GalleryController(IPhotoQueryService photos)
        {
            _photos = photos;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _photos.GetLatestAsync(10);
            return View(items);
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

            var criteria = new PhotoSearchCriteriaDto();

            if (query.StartsWith("#"))
            {
                criteria.Hashtag = query.TrimStart('#').ToLowerInvariant();
            }
            else
            {
                criteria.AuthorEmail = query;
            }

            var results = await _photos.SearchAsync(criteria);
            return PartialView("_SearchResults", results);
        }


    }
}
