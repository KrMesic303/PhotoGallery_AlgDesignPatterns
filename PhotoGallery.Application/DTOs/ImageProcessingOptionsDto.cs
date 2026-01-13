namespace PhotoGallery.Application.DTOs
{
    namespace PhotoGallery.Application.DTOs
    {
        public class ImageProcessingOptionsDto
        {
            public int? ResizeWidth { get; set; }
            public int? ResizeHeight { get; set; }
            public string? OutputFormat { get; set; } // "jpg", "png", "bmp"
            public bool ApplySepia { get; set; }
            public float? BlurAmount { get; set; }
        }
    }

}
