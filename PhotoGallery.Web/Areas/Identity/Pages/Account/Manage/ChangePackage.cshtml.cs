using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Web.Areas.Identity.Pages.Account.Manage
{
    public class ChangePackageModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ChangePackageModel(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public int SelectedPackageId { get; set; }

        public List<SelectListItem> PackageOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found.");

            // Load package list
            PackageOptions = await _context.PackagePlans
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToListAsync();

            SelectedPackageId = user.PackagePlanId ?? 0;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found.");

            // TODO: we can add business rule to change package daily... we can move this to configuration file later
            var now = DateTime.UtcNow;
            if ((now - user.PackageEffectiveFrom).TotalDays < 0)
            {
                StatusMessage = "You can only change your package once per day.";
                return RedirectToPage();
            }

            user.PackagePlanId = SelectedPackageId;
            user.PackageEffectiveFrom = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            StatusMessage = "Your package change is scheduled for tomorrow.";
            return RedirectToPage("./Index");
        }
    }
}
