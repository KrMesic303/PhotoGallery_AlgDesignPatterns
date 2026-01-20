namespace PhotoGallery.Application.Events.Photos
{
    public sealed class PhotoDownloadedEvent : IDomainEvent
    {
        public PhotoDownloadedEvent(int photoId, string userIdOrAnonymous, bool original)
        {
            PhotoId = photoId;
            UserIdOrAnonymous = userIdOrAnonymous;
            Original = original;
            OccurredAtUtc = DateTime.UtcNow;
        }

        public int PhotoId { get; }
        public string UserIdOrAnonymous { get; }
        public bool Original { get; }
        public DateTime OccurredAtUtc { get; }
    }
}
