namespace PhotoGallery.Application.Events.Admin
{
    public sealed class PackageChangedEvent(string adminUserId, string targetUserId, int packageId) : IDomainEvent
    {
        public string AdminUserId { get; } = adminUserId;
        public string TargetUserId { get; } = targetUserId;
        public int PackageId { get; } = packageId;
        public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    }
}
