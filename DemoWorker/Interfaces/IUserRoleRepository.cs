using DemoWorker.Entities;

namespace DemoWorker.Interfaces;

public interface IUserRoleRepository
{
    Task<int> DeleteAllAsync();
    Task<int> BulkInsertAsync(IEnumerable<UserRole> userRoles);
    Task<int> ReplaceAllAsync(IEnumerable<UserRole> userRoles);
}