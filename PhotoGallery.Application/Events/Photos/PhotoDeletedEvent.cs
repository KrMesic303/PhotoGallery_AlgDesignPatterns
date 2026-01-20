namespace PhotoGallery.Application.Events.Photos
{
    public sealed class PhotoDeletedEvent(int photoId, string userId, bool isAdminAction) : IDomainEvent
    {
        public int PhotoId { get; } = photoId;
        public string UserId { get; } = userId;
        public bool IsAdminAction { get; } = isAdminAction;
        public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    }
}
