using System.Data;

namespace DemoWorker.Interfaces;

public interface IDapperContext
{
    IDbConnection CreateConnection();
    Task<T> ExecuteInTransactionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> operation);
}