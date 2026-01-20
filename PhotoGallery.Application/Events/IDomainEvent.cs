namespace PhotoGallery.Application.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredAtUtc { get; }
    }
}
