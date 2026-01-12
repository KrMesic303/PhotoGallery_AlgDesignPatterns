namespace PhotoGallery.Domain.Entities
{
    public class Photo
    {
        public int Id { get; set; }

        public string StorageKey { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long SizeInBytes { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public ICollection<PhotoHashtag> Hashtags { get; set; } = new List<PhotoHashtag>();
        public string ThumbnailStorageKey { get; set; } = string.Empty;
    }
}
