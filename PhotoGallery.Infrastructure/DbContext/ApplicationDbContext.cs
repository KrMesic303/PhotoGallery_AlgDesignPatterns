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

        // Repository pattern (SRP)
        public DbSet<PackagePlan> PackagePlans => Set<PackagePlan>();

        // Photos and Hashtags
        public DbSet<Photo> Photos => Set<Photo>();
        public DbSet<Hashtag> Hashtags => Set<Hashtag>();
        public DbSet<PhotoHashtag> PhotoHashtags => Set<PhotoHashtag>();
        public DbSet<PhotoFilter> PhotoFilters => Set<PhotoFilter>();

        // Audit Logs
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<AppMetric> AppMetrics => Set<AppMetric>();


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

            // Photos relationships
            builder.Entity<PhotoHashtag>()
                .HasKey(ph => new { ph.PhotoId, ph.HashtagId });

            builder.Entity<PhotoHashtag>()
                .HasOne(ph => ph.Photo)
                .WithMany(p => p.Hashtags)
                .HasForeignKey(ph => ph.PhotoId);

            builder.Entity<PhotoHashtag>()
                .HasOne(ph => ph.Hashtag)
                .WithMany()
                .HasForeignKey(ph => ph.HashtagId);

            builder.Entity<PhotoFilter>()
                .HasIndex(f => f.FilterType);

            builder.Entity<AppMetric>()
                .HasIndex(m => m.Key)
                .IsUnique();

            var seedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.Entity<AppMetric>().HasData(
                new AppMetric { Id = 1, Key = "TOTAL_UPLOADS", Value = 0, UpdatedAtUtc = seedTime },
                new AppMetric { Id = 2, Key = "TOTAL_DOWNLOADS", Value = 0, UpdatedAtUtc = seedTime },
                new AppMetric { Id = 3, Key = "TOTAL_DELETES", Value = 0, UpdatedAtUtc = seedTime }
            );


        }
    }
}
