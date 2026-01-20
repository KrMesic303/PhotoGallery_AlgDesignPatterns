using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.Abstractions.Repositories
{
    /// <summary>
    /// PATTERN: Repository
    /// </summary>
    public interface IHashtagRepository
    {
        Task<Hashtag> GetOrCreateAsync(string normalizedValue, CancellationToken cancellationToken = default);
    }
}
