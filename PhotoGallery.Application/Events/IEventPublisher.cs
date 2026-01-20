namespace PhotoGallery.Application.Events
{
    /// <summary>
    /// PATTERN: Observer
    /// </summary>
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    }
}
