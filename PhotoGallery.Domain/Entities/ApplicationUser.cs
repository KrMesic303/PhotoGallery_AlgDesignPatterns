using Microsoft.AspNetCore.Identity;

namespace PhotoGallery.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public int? PackagePlanId { get; set; }
        public PackagePlan? PackagePlan { get; set; }

        public DateTime PackageEffectiveFrom { get; set; } = DateTime.UtcNow;
    }
}
