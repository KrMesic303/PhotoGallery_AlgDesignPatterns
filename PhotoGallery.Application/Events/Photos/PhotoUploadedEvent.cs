namespace PhotoGallery.Application.Events.Photos
{
    public sealed class PhotoUploadedEvent(int photoId, string userId) : IDomainEvent
    {
        public int PhotoId { get; } = photoId;
        public string UserId { get; } = userId;
        public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    }
}
