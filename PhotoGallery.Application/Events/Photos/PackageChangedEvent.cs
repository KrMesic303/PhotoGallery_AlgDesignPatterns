namespace PhotoGallery.Application.Events.Admin
{
    public sealed class PackageChangedEvent : IDomainEvent
    {
        public PackageChangedEvent(string adminUserId, string targetUserId, int packageId)
        {
            AdminUserId = adminUserId;
            TargetUserId = targetUserId;
            PackageId = packageId;
            OccurredAtUtc = DateTime.UtcNow;
        }

        public string AdminUserId { get; }
        public string TargetUserId { get; }
        public int PackageId { get; }
        public DateTime OccurredAtUtc { get; }
    }
}
