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
        public IActionResult Search()
        {
            return View(new PhotoSearchCriteriaDto());
        }

        [HttpPost]
        public async Task<IActionResult> Search(PhotoSearchCriteriaDto criteria)
        {
            var results = await _photos.SearchAsync(criteria);
            return View("SearchResults", results);
        }


    }
}
