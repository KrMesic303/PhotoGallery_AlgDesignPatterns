using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Domain.Entities;
using PhotoGallery.Infrastructure.DbContext;
using PhotoGallery.Infrastructure.ImageProcessing;
using PhotoGallery.Infrastructure.Logging;
using PhotoGallery.Infrastructure.Queries;
using PhotoGallery.Infrastructure.Services;
using PhotoGallery.Infrastructure.Storage;

namespace PhotoGallery.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC
            builder.Services.AddControllersWithViews();

            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
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

            // External Authentication
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                })
                .AddGitHub(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
                    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
                    options.Scope.Add("user:email");
                });

            // Strategy pattern - switching between different behaviours (storage, policies, logging)
            builder.Services.AddScoped<IImageProcessorFactory, ImageProcessorFactory>();
            builder.Services.AddScoped<IAuditLogger, AuditLogger>();
            builder.Services.AddScoped<IUploadQuotaService, UploadQuotaService>();
            builder.Services.AddScoped<IPhotoUploadPolicy, PhotoUploadPolicy>();
            builder.Services.AddScoped<IPhotoQueryService, PhotoQueryService>();

            // Storage abstraction (can add S3 storage, or example like Google cloud storage)
            // PATTERN: Factory pattern (DI registrations) - Storage and image processing configuraiton
            var provider = builder.Configuration["Storage:Provider"] ?? "Local";

            if (provider.Equals("Gcs", StringComparison.OrdinalIgnoreCase))
            {
                builder.Services.AddScoped<IPhotoStorageService, PhotoGallery.Infrastructure.Storage.GcsPhotoStorageService>();
            }
            else
            {
                builder.Services.AddScoped<IPhotoStorageService, LocalPhotoStorageService>();
            }

            var app = builder.Build();

            // Seed Roles / Admin user
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                const string adminRole = "Administrator";

                if (!await roleManager.RoleExistsAsync(adminRole))
                {
                    await roleManager.CreateAsync(new IdentityRole(adminRole));
                }

                var adminEmail = "admin@test.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, adminRole))
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
            }

            // Middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Gallery}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapRazorPages(); // register / login pages

            app.Run();

        }
    }
}
