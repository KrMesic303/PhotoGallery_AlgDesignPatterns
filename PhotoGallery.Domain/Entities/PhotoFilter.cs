namespace PhotoGallery.Domain.Entities
{
    public class PhotoFilter
    {
        public int Id { get; set; }

        public int PhotoId { get; set; }
        public Photo Photo { get; set; } = null!;

        public string FilterType { get; set; } = null!;
        public string FilterValue { get; set; } = null!;
    }
}
