namespace DemoWorker.Interfaces;

public interface IUserRoleSyncService
{
    Task<bool> SyncUserRolesAsync(CancellationToken cancellationToken = default);
}