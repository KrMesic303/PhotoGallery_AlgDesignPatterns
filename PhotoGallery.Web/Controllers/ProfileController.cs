using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Application.Abstractions.Queries;
using System.Security.Claims;

namespace PhotoGallery.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserProfileQueryService _profiles;

        public ProfileController(IUserProfileQueryService profiles)
        {
            _profiles = profiles;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var user = await _profiles.GetUserWithPlanAsync(userId, ct);
            if (user == null)
                return NotFound();

            return View(user);
        }
    }
}
