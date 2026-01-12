namespace PhotoGallery.Application.Abstractions
{
    public class UploadPolicyResult
    {
        public bool IsAllowed { get; init; }
        public string? ErrorMessage { get; init; }

        public static UploadPolicyResult Allow() => new() { IsAllowed = true };

        public static UploadPolicyResult Deny(string message) => new() { IsAllowed = false, ErrorMessage = message };
    }
}
