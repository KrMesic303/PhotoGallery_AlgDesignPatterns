using Microsoft.Extensions.DependencyInjection;
using PhotoGallery.Application.Events;

namespace PhotoGallery.Infrastructure.Events
{
    /// <summary>
    /// PATTERN: Observer
    /// </summary>
    public sealed class InProcessEventPublisher : IEventPublisher
    {
        private readonly IServiceProvider _serviceProvider;

        public InProcessEventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
            where TEvent : IDomainEvent
        {
            var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>();

            foreach (var handler in handlers)
            {
                await handler.HandleAsync(domainEvent, cancellationToken);
            }
        }
    }
}
