namespace PhotoGallery.Application.Events.Photos
{
    public sealed class PhotoDeletedEvent : IDomainEvent
    {
        public PhotoDeletedEvent(int photoId, string userId, bool isAdminAction)
        {
            PhotoId = photoId;
            UserId = userId;
            IsAdminAction = isAdminAction;
            OccurredAtUtc = DateTime.UtcNow;
        }

        public int PhotoId { get; }
        public string UserId { get; }
        public bool IsAdminAction { get; }
        public DateTime OccurredAtUtc { get; }
    }
}
