using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Application.UseCases.Files;

namespace PhotoGallery.Web.Controllers
{
    public class FilesController : Controller
    {
        private readonly IGetPhotoFileHandler _files;

        public FilesController(IGetPhotoFileHandler files)
        {
            _files = files;
        }

        [HttpGet]
        public async Task<IActionResult> Photo(int id, CancellationToken ct)
        {
            try
            {
                var result = await _files.HandleAsync(
                    new GetPhotoFileQuery { PhotoId = id, IsThumbnail = false },
                    ct);

                return File(result.Stream, result.ContentType);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Thumbnail(int id, CancellationToken ct)
        {
            try
            {
                var result = await _files.HandleAsync(
                    new GetPhotoFileQuery { PhotoId = id, IsThumbnail = true },
                    ct);

                return File(result.Stream, result.ContentType);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
