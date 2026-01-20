using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.Abstractions.Repositories
{
    /// <summary>
    /// PATTERN: Repository
    /// </summary>
    public interface IPhotoRepository
    {
        Task<Photo?> FindAsync(int id, CancellationToken cancellationToken = default);

        Task<Photo?> GetWithHashtagsAsync(int id, CancellationToken cancellationToken = default);

        void Add(Photo photo);

        void Remove(Photo photo);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
