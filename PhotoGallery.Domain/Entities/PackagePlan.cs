namespace PhotoGallery.Domain.Entities
{
    public class PackagePlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long MaxPhotoSizeBytes { get; set; }
        public int DailyUploadLimit { get; set; }
        public long MaxStorageBytes { get; set; }

        //TODO: add pricing later...

        public ICollection<ApplicationUser>? Users { get; set; }
    }
}
