using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Infrastructure.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<PackagePlan> PackagePlans => Set<PackagePlan>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PackagePlan>().HasData(
                new PackagePlan
                {
                    Id = 1,
                    Name = "FREE",
                    MaxPhotoSizeBytes = 5 * 1024 * 1024, // 5 MB
                    DailyUploadLimit = 5,
                    MaxStorageBytes = 100 * 1024 * 1024 // 100 MB
                },
                new PackagePlan
                {
                    Id = 2,
                    Name = "PRO",
                    MaxPhotoSizeBytes = 20 * 1024 * 1024, // 20 MB
                    DailyUploadLimit = 100,
                    MaxStorageBytes = 5L * 1024 * 1024 * 1024 // 5 GB
                },
                new PackagePlan
                {
                    Id = 3,
                    Name = "GOLD",
                    MaxPhotoSizeBytes = 100 * 1024 * 1024, // 100 MB
                    DailyUploadLimit = 100,
                    MaxStorageBytes = 10L * 1024 * 1024 * 1024 // 10 GB
                }
            );
        }
    }
}
