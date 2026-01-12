namespace PhotoGallery.Domain.Entities
{
    public class PhotoHashtag
    {
        public int PhotoId { get; set; }
        public Photo Photo { get; set; } = default!;

        public int HashtagId { get; set; }
        public Hashtag Hashtag { get; set; } = default!;
    }
}
