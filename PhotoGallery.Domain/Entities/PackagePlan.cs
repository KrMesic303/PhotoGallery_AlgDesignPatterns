namespace PhotoGallery.Domain.Entities
{
    public class PackagePlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public long MaxPhotoSizeBytes { get; set; }
        public int DailyUploadLimit { get; set; }
        public long MaxStorageBytes { get; set; }
    }
}
