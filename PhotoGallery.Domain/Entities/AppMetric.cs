namespace PhotoGallery.Domain.Entities
{
    public class AppMetric
    {
        public int Id { get; set; }

        public string Key { get; set; } = string.Empty;
        public long Value { get; set; }

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
