namespace PhotoGallery.Application.DTOs
{
    public class PhotoDownloadStat
    {
        public int PhotoId { get; set; }
        public string Description { get; set; } = "";
        public int DownloadCount { get; set; }
    }
}
