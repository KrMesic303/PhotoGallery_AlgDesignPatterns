namespace PhotoGallery.Application.Abstractions.Metrics
{
    public interface IAppMetricStore
    {
        Task IncrementAsync(string key, long delta, CancellationToken cancellationToken = default);
    }
}
