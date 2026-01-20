using Microsoft.AspNetCore.Identity;
using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.UseCases.Admin.ChangePackage;
using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Infrastructure.UseCases.Admin.ChangePackage
{
    public sealed class ChangePackageHandler : IChangePackageHandler
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogger _audit;

        public ChangePackageHandler(UserManager<ApplicationUser> userManager, IAuditLogger audit)
        {
            _userManager = userManager;
            _audit = audit;
        }

        public async Task HandleAsync(ChangePackageCommand command, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(command.TargetUserId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            user.PackagePlanId = command.PackageId;
            user.PackageEffectiveFrom = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException("Failed to change package: " + msg);
            }

            await _audit.LogAsync(
                command.AdminUserId,
                "ADMIN_CHANGE_PACKAGE",
                nameof(ApplicationUser),
                command.TargetUserId,
                cancellationToken);
        }
    }
}
