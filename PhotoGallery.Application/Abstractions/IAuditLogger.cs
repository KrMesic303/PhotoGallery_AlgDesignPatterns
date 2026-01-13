namespace PhotoGallery.Application.Abstractions
{
    /// <summary>
    /// PATTERN: Strategy pattern
    /// </summary>
    public interface IAuditLogger
    {
        Task LogAsync(string userId, string action, string? entityType = null, string? entityId = null, CancellationToken cancellationToken = default);
    }
}
