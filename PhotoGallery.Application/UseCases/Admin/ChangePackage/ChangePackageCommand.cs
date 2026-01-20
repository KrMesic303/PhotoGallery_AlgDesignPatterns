namespace PhotoGallery.Application.UseCases.Admin.ChangePackage
{
    public sealed class ChangePackageCommand
    {
        public required string TargetUserId { get; init; }
        public required int PackageId { get; init; }

        public required string AdminUserId { get; init; } // audit
    }
}
