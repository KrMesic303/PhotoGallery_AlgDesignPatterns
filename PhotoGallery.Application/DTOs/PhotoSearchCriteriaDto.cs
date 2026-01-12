namespace PhotoGallery.Application.DTOs
{
    public class PhotoSearchCriteriaDto
    {
        public string? Hashtag { get; set; }
        public string? AuthorEmail { get; set; }
        public DateTime? UploadedFrom { get; set; }
        public DateTime? UploadedTo { get; set; }
        public long? MinSizeBytes { get; set; }
        public long? MaxSizeBytes { get; set; }
    }
}
