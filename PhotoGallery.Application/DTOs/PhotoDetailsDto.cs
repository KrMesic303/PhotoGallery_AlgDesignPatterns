namespace PhotoGallery.Application.DTOs
{
    public class PhotoDetailsDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public List<string> Hashtags { get; set; } = new();
    }
}
