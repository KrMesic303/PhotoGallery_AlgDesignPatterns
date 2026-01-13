using PhotoGallery.Application.DTOs;

namespace PhotoGallery.Web.ViewModels
{
    public class AdminStatisticsViewModel
    {
        public int Users { get; set; }
        public int Photos { get; set; }
        public long StorageUsed { get; set; }

        // Uploads
        public int TotalUploads { get; set; }
        public int UploadsLast7Days { get; set; }
        public long LargestPhotoSize { get; set; }
        public double AveragePhotoSize { get; set; }

        // Downloads
        public int TotalDownloads { get; set; }
        public List<PhotoDownloadStat> TopDownloadedPhotos { get; set; } = [];

        // PerUser
        public List<UserStorageStat> StoragePerUser { get; set; } = [];
    }

}
