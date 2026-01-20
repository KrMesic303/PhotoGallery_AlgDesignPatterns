namespace PhotoGallery.Application.UseCases.Admin.ChangePackage
{
    public interface IChangePackageHandler
    {
        Task HandleAsync(ChangePackageCommand command, CancellationToken cancellationToken = default);
    }
}
