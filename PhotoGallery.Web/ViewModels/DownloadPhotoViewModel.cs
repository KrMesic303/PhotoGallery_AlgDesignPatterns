using System.ComponentModel.DataAnnotations;

namespace PhotoGallery.Web.ViewModels
{
    public class DownloadPhotoViewModel
    {
        public int PhotoId { get; set; }

        public bool DownloadOriginal { get; set; } = true;

        [Display(Name = "Resize (px)")]
        public int? Resize { get; set; }

        [Display(Name = "Format")]
        public string Format { get; set; } = "jpg";

        public bool ApplySepia { get; set; }
        public float? BlurAmount { get; set; }
    }
}
