using PhotoGallery.Application.Abstractions;
using PhotoGallery.Application.Events;
using PhotoGallery.Application.Events.Admin;
using PhotoGallery.Application.UseCases.Admin.ChangePackage;
using PhotoGallery.Domain.Entities;

namespace PhotoGallery.Application.UseCases.Common.Auditing
{
    /// <summary>
    /// PATTERN: Decorator
    /// </summary>
    public sealed class AuditedChangePackageHandler : IChangePackageHandler
    {
        private readonly IChangePackageHandler _inner;
        private readonly IAuditLogger _audit;
        private readonly IEventPublisher _events;

        public AuditedChangePackageHandler(IChangePackageHandler inner, IAuditLogger audit, IEventPublisher events)
        {
            _inner = inner;
            _audit = audit;
            _events = events;
        }

        public async Task HandleAsync(ChangePackageCommand command, CancellationToken cancellationToken = default)
        {
            await _inner.HandleAsync(command, cancellationToken);

            await _events.PublishAsync(new PackageChangedEvent(command.AdminUserId, command.TargetUserId, command.PackageId), cancellationToken);

            await _audit.LogAsync(
                userId: command.AdminUserId,
                action: "ADMIN_CHANGE_PACKAGE",
                entityType: nameof(ApplicationUser),
                entityId: command.TargetUserId,
                cancellationToken: cancellationToken);
        }
    }
}
