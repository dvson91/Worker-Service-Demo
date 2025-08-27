using System.Data;
using System.Data.SqlClient;
using Dapper;
using DemoWorker.Entities;
using DemoWorker.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DemoWorker.Repositories;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly string _connectionString;
    private readonly ILogger<UserRoleRepository> _logger;

    public UserRoleRepository(IConfiguration configuration, ILogger<UserRoleRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string not found");
        _logger = logger;
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<IEnumerable<UserRole>> GetAllAsync()
    {
        const string sql = "SELECT Id, DomainId, Role, Module, CreatedAt, UpdatedAt FROM UserRoles";

        using var connection = CreateConnection();
        return await connection.QueryAsync<UserRole>(sql);
    }

    public async Task<UserRole?> GetByIdAsync(int id)
    {
        const string sql = "SELECT Id, DomainId, Role, Module, CreatedAt, UpdatedAt FROM UserRoles WHERE Id = @Id";

        using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserRole>(sql, new { Id = id });
    }

    public async Task<IEnumerable<UserRole>> GetByModuleAsync(string module)
    {
        const string sql = "SELECT Id, DomainId, Role, Module, CreatedAt, UpdatedAt FROM UserRoles WHERE Module = @Module";

        using var connection = CreateConnection();
        return await connection.QueryAsync<UserRole>(sql, new { Module = module });
    }

    public async Task<int> CreateAsync(UserRole userRole)
    {
        const string sql = @"
            INSERT INTO UserRoles (DomainId, Role, Module, CreatedAt, UpdatedAt) 
            VALUES (@DomainId, @Role, @Module, @CreatedAt, @UpdatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        userRole.CreatedAt = DateTime.UtcNow;
        userRole.UpdatedAt = DateTime.UtcNow;

        using var connection = CreateConnection();
        var id = await connection.QuerySingleAsync<int>(sql, userRole);
        userRole.Id = id;
        return id;
    }

    public async Task<int> UpdateAsync(UserRole userRole)
    {
        const string sql = @"
            UPDATE UserRoles 
            SET DomainId = @DomainId, Role = @Role, Module = @Module, UpdatedAt = @UpdatedAt 
            WHERE Id = @Id";

        userRole.UpdatedAt = DateTime.UtcNow;

        using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql, userRole);
    }

    public async Task<int> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM UserRoles WHERE Id = @Id";

        using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<int> DeleteAllAsync()
    {
        const string sql = "DELETE FROM UserRoles";

        using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql);
    }

    public async Task<int> BulkInsertAsync(IEnumerable<UserRole> userRoles)
    {
        if (!userRoles.Any())
        {
            return 0;
        }

        const string sql = @"
            INSERT INTO UserRoles (DomainId, Role, Module, CreatedAt, UpdatedAt) 
            VALUES (@DomainId, @Role, @Module, @CreatedAt, @UpdatedAt)";

        var now = DateTime.UtcNow;
        foreach (var userRole in userRoles)
        {
            userRole.CreatedAt = now;
            userRole.UpdatedAt = now;
        }

        using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql, userRoles);
    }

    public async Task<bool> ExistsAsync(string domainId, string role, string module)
    {
        const string sql = "SELECT 1 FROM UserRoles WHERE DomainId = @DomainId AND Role = @Role AND Module = @Module";

        using var connection = CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync<int?>(sql, new { DomainId = domainId, Role = role, Module = module });
        return result.HasValue;
    }
}