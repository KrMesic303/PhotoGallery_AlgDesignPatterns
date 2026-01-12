namespace PhotoGallery.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string? EntityType { get; set; }

        public string? EntityId { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
