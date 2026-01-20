namespace PhotoGallery.Application.Abstractions.Storage
{
    /// <summary>
    /// Pattern: Abstract Factory
    /// </summary>
    public interface IStorageProviderFactory
    {
        IPhotoStorageService CreatePhotoStorageService();
    }
}
