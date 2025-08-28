using System.Data;
using System.Data.SqlClient;
using DemoWorker.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DemoWorker.Data;

public class DapperContext : IDapperContext
{
    private readonly string _connectionString;
    private readonly ILogger<DapperContext> _logger;

    public DapperContext(IConfiguration configuration, ILogger<DapperContext> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string not found");
        _logger = logger;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> operation)
    {
        using var connection = CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            var result = await operation(connection, transaction);
            transaction.Commit();

            _logger.LogDebug("Transaction completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Transaction failed and was rolled back");
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<IDbConnection, IDbTransaction, Task> operation)
    {
        using var connection = CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            await operation(connection, transaction);
            transaction.Commit();

            _logger.LogDebug("Transaction completed successfully");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Transaction failed and was rolled back");
            throw;
        }
    }
}