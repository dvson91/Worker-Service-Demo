using DemoWorker.Entities;

namespace DemoWorker.Interfaces;

public interface IUserRoleRepository
{
    Task<IEnumerable<UserRole>> GetAllAsync();
    Task<UserRole?> GetByIdAsync(int id);
    Task<IEnumerable<UserRole>> GetByModuleAsync(string module);
    Task<int> CreateAsync(UserRole userRole);
    Task<int> UpdateAsync(UserRole userRole);
    Task<int> DeleteAsync(int id);
    Task<int> DeleteAllAsync();
    Task<int> BulkInsertAsync(IEnumerable<UserRole> userRoles);
    Task<bool> ExistsAsync(string domainId, string role, string module);
}