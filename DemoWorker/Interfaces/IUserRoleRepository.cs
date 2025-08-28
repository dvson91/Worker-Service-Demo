using DemoWorker.Entities;

namespace DemoWorker.Interfaces;

public interface IUserRoleRepository
{
    Task<int> ReplaceAllAsync(IEnumerable<UserRole> userRoles);
}