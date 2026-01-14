namespace PhotoGallery.Application.DTOs.Admin
{
    public sealed class AdminStatisticsDto
    {
        public int Users { get; set; }
        public int Photos { get; set; }
        public long StorageUsed { get; set; }

        public int TotalUploads { get; set; }
        public int UploadsLast7Days { get; set; }

        public long LargestPhotoSize { get; set; }
        public double AveragePhotoSize { get; set; }

        public int TotalDownloads { get; set; }
        public List<PhotoDownloadStatDto> TopDownloadedPhotos { get; set; } = new();
        public List<UserStorageStatDto> StoragePerUser { get; set; } = new();
    }

    public sealed class PhotoDownloadStatDto
    {
        public int PhotoId { get; set; }
        public int DownloadCount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public sealed class UserStorageStatDto
    {
        public string UserEmail { get; set; } = string.Empty;
        public long StorageUsed { get; set; }
    }
}
