using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs;
using PhotoGallery.Infrastructure.DbContext;

namespace PhotoGallery.Infrastructure.Queries
{
    /// <summary>
    /// Specification pattern
    /// </summary>
    public class PhotoQueryService : IPhotoQueryService
    {
        private readonly ApplicationDbContext _context;

        public PhotoQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PhotoListItemDto>> GetLatestAsync(int count)
        {
            return await _context.Photos
                .OrderByDescending(p => p.UploadedAtUtc)
                .Take(count)
                .Select(p => new PhotoListItemDto
                {
                    Id = p.Id,
                    // TODO: fallback for now, later we can return it as thumbnails will be added
                    ThumbnailUrl = string.IsNullOrEmpty(p.ThumbnailStorageKey)
                        ? "/images/no-thumbnail.png"
                        : "/uploads/" + p.ThumbnailStorageKey,
                    Description = p.Description,
                    AuthorEmail = p.User.Email!,
                    UploadedAt = p.UploadedAtUtc,
                    Hashtags = p.Hashtags.Select(h => h.Hashtag.Value).ToList()
                })
                .ToListAsync();
        }

        public async Task<PhotoDetailsDto?> GetDetailsAsync(int photoId)
        {
            return await _context.Photos
                .Where(p => p.Id == photoId)
                .Select(p => new PhotoDetailsDto
                {
                    Id = p.Id,
                    ImageUrl = "/uploads/" + p.StorageKey,
                    Description = p.Description,
                    AuthorEmail = p.User.Email!,
                    UploadedAt = p.UploadedAtUtc,
                    Hashtags = p.Hashtags.Select(h => h.Hashtag.Value).ToList()
                })
                .FirstOrDefaultAsync();
        }
    }
}
