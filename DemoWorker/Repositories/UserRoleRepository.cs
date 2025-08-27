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

    public async Task<int> DeleteAllAsync()
    {
        const string sql = "TRUNCATE TABLE UserRoles";

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql);

        _logger.LogDebug("Truncated UserRoles table");
        return 0; // TRUNCATE doesn't return affected row count
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
}