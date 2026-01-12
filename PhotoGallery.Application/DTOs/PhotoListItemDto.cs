namespace PhotoGallery.Application.DTOs
{
    public class PhotoListItemDto
    {
        public int Id { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public List<string> Hashtags { get; set; } = new();
    }
}
