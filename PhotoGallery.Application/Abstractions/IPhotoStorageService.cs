namespace PhotoGallery.Application.Abstractions
{
    /// <summary>
    /// PATTERN: Strategy
    /// SOLID: Open/Closed & DI principle
    /// </summary>
    public interface IPhotoStorageService
    {
        Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);

        Task<Stream> GetAsync(string storageKey, CancellationToken cancellationToken = default);

        Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
    }
}
