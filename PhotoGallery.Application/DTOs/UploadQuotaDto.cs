namespace PhotoGallery.Application.DTOs
{
    public class UploadQuotaDto
    {
        public int RemainingUploadsToday { get; set; }
        public long RemainingStorageBytes { get; set; }
        public long MaxPhotoSizeBytes { get; set; }
    }
}
