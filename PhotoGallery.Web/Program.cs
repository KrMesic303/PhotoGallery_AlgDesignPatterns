using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;
using PhotoGallery.Infrastructure.Logging;
using PhotoGallery.Infrastructure.Queries;
using PhotoGallery.Infrastructure.Services;
using PhotoGallery.Infrastructure.Storage;

namespace PhotoGallery.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC
            builder.Services.AddControllersWithViews();

            // DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity (on .NET 10)
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
#if DEBUG
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
#endif
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            // Strategy pattern - switching between different behaviours (storage, policies, logging)
            builder.Services.AddScoped<IAuditLogger, AuditLogger>();
            builder.Services.AddScoped<IUploadQuotaService, UploadQuotaService>();
            builder.Services.AddScoped<IPhotoUploadPolicy, PhotoUploadPolicy>();
            builder.Services.AddScoped<IPhotoQueryService, PhotoQueryService>();

            // Factory pattern (DI registrations) - Storage and image processing configuraiton
            builder.Services.AddScoped<IPhotoStorageService, LocalPhotoStorageService>();

            var app = builder.Build();

            // Middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapRazorPages(); // register / login pages

            app.Run();
        }
    }
}
