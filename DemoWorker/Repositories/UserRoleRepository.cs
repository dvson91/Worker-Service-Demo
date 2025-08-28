using System.Data;
using Dapper;
using DemoWorker.Entities;
using DemoWorker.Interfaces;

namespace DemoWorker.Repositories;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly IDapperContext _dapperContext;
    private readonly ILogger<UserRoleRepository> _logger;

    public UserRoleRepository(IDapperContext dapperContext, ILogger<UserRoleRepository> logger)
    {
        _dapperContext = dapperContext ?? throw new ArgumentNullException(nameof(dapperContext));
        _logger = logger;
    }

    public async Task<int> DeleteAllAsync()
    {
        const string sql = "TRUNCATE TABLE UserRoles";

        using var connection = _dapperContext.CreateConnection();
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

        using var connection = _dapperContext.CreateConnection();
        return await connection.ExecuteAsync(sql, userRoles);
    }

    public async Task<int> ReplaceAllAsync(IEnumerable<UserRole> userRoles)
    {
        return await _dapperContext.ExecuteInTransactionAsync(async (connection, transaction) =>
        {
            // First truncate the table
            const string truncateSql = "TRUNCATE TABLE UserRoles";
            await connection.ExecuteAsync(truncateSql, transaction: transaction);

            _logger.LogDebug("Truncated UserRoles table within transaction");

            // If no new data, return 0
            if (!userRoles.Any())
            {
                _logger.LogDebug("No user roles to insert");
                return 0;
            }

            // Then insert new data
            const string insertSql = @"
                INSERT INTO UserRoles (DomainId, Role, Module, CreatedAt, UpdatedAt) 
                VALUES (@DomainId, @Role, @Module, @CreatedAt, @UpdatedAt)";

            var now = DateTime.UtcNow;
            foreach (var userRole in userRoles)
            {
                userRole.CreatedAt = now;
                userRole.UpdatedAt = now;
            }

            var insertedCount = await connection.ExecuteAsync(insertSql, userRoles, transaction: transaction);

            _logger.LogDebug("Inserted {Count} user roles within transaction", insertedCount);
            return insertedCount;
        });
    }
}