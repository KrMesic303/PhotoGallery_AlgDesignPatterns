using System.ComponentModel.DataAnnotations;

namespace PhotoGallery.Web.ViewModels
{
    public class EditPhotoViewModel
    {
        public int PhotoId { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Hashtags (comma separated)")]
        public string Hashtags { get; set; } = string.Empty;
    }
}
