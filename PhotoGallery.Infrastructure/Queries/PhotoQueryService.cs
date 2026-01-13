using Microsoft.EntityFrameworkCore;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.DTOs;
using PhotoGallery.Domain.Entities;
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
                        : $"/Files/Thumbnail/{p.Id}",
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
                    ImageUrl = $"/Files/Photo/{p.Id}",
                    Description = p.Description,
                    AuthorEmail = p.User.Email!,
                    UploadedAt = p.UploadedAtUtc,
                    AuthorId = p.UserId,
                    Hashtags = p.Hashtags.Select(h => h.Hashtag.Value).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<PhotoListItemDto>> SearchAsync(PhotoSearchCriteriaDto criteria)
        {
            var query = _context.Photos
                .Include(p => p.User)
                .Include(p => p.Hashtags)
                .ThenInclude(ph => ph.Hashtag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(criteria.AuthorEmail))
            {
                query = query.Where(p => p.User.Email!.Contains(criteria.AuthorEmail));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Hashtag))
            {
                var hashtag = criteria.Hashtag.ToLowerInvariant();
                query = query.Where(p =>
                    p.Hashtags.Any(h =>
                        EF.Functions.Like(h.Hashtag.Value, $"%{hashtag}%")));
            }

            if (criteria.UploadedFrom.HasValue)
            {
                query = query.Where(p => p.UploadedAtUtc >= criteria.UploadedFrom.Value);
            }

            if (criteria.UploadedTo.HasValue)
            {
                query = query.Where(p => p.UploadedAtUtc <= criteria.UploadedTo.Value);
            }

            if (criteria.MinSizeBytes.HasValue)
            {
                query = query.Where(p => p.SizeInBytes >= criteria.MinSizeBytes.Value);
            }

            if (criteria.MaxSizeBytes.HasValue)
            {
                query = query.Where(p => p.SizeInBytes <= criteria.MaxSizeBytes.Value);
            }

            return await query
                .OrderByDescending(p => p.UploadedAtUtc)
                .Select(p => new PhotoListItemDto
                {
                    Id = p.Id,
                     ThumbnailUrl = string.IsNullOrEmpty(p.ThumbnailStorageKey)
                        ? "/images/no-thumbnail.png"
                        : $"/Files/Thumbnail/{p.Id}",
                    Description = p.Description,
                    AuthorEmail = p.User.Email!,
                    UploadedAt = p.UploadedAtUtc,
                    Hashtags = p.Hashtags.Select(h => h.Hashtag.Value).ToList()
                })
                .ToListAsync();
        }
    }
}
