namespace PhotoGallery.Application.Events.Photos
{
    public sealed class PhotoDownloadedEvent(int photoId, string userIdOrAnonymous, bool original) : IDomainEvent
    {
        public int PhotoId { get; } = photoId;
        public string UserIdOrAnonymous { get; } = userIdOrAnonymous;
        public bool Original { get; } = original;
        public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    }
}
