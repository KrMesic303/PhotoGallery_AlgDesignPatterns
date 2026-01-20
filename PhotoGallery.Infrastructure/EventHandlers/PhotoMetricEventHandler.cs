using PhotoGallery.Application.Abstractions.Metrics;
using PhotoGallery.Application.Events;
using PhotoGallery.Application.Events.Photos;

namespace PhotoGallery.Infrastructure.EventHandlers
{
    /// <summary>
    /// PATTERN: Observer - Concrete
    /// </summary>
    public sealed class PhotoMetricEventHandler : IDomainEventHandler<PhotoUploadedEvent>, IDomainEventHandler<PhotoDownloadedEvent>, IDomainEventHandler<PhotoDeletedEvent>
    {
        private readonly IAppMetricStore _metrics;

        public PhotoMetricEventHandler(IAppMetricStore metrics)
        {
            _metrics = metrics;
        }

        public Task HandleAsync(PhotoUploadedEvent domainEvent, CancellationToken cancellationToken = default) => _metrics.IncrementAsync("TOTAL_UPLOADS", 1, cancellationToken);

        public Task HandleAsync(PhotoDownloadedEvent domainEvent, CancellationToken cancellationToken = default) => _metrics.IncrementAsync("TOTAL_DOWNLOADS", 1, cancellationToken);

        public Task HandleAsync(PhotoDeletedEvent domainEvent, CancellationToken cancellationToken = default) => _metrics.IncrementAsync("TOTAL_DELETES", 1, cancellationToken);
    }
}
