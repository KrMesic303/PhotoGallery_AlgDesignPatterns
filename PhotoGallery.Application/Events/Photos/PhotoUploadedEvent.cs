namespace PhotoGallery.Application.Events.Photos
{
    public sealed class PhotoUploadedEvent : IDomainEvent
    {
        public PhotoUploadedEvent(int photoId, string userId)
        {
            PhotoId = photoId;
            UserId = userId;
            OccurredAtUtc = DateTime.UtcNow;
        }

        public int PhotoId { get; }
        public string UserId { get; }
        public DateTime OccurredAtUtc { get; }
    }
}
