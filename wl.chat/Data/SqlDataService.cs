using Microsoft.Data.SqlClient;
using Dapper;

namespace wl.chat.Data;

public class SqlDataService : ISqlDataService
{
    private readonly IConfiguration _config;

    public SqlDataService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<TParam>> LoadData<TParam, TResult>(string storedProcedure, TResult parameters, string connectionStringName = "DefaultConnection")
    {
        string connectionString = _config.GetConnectionString(connectionStringName) ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

        using IDbConnection connection = new SqlConnection(connectionString);

        var rows = await connection.QueryAsync<TParam>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

        return rows;
    }

    public async Task SaveData<TParam>(string storedProcedure, TParam parameters, string connectionStringName = "DefaultConnection")
    {
        string connectionString = _config.GetConnectionString(connectionStringName) ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

        using IDbConnection connection = new SqlConnection(connectionString);

        await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<TResult> LoadMultipleData<TParam, TResult>(string storedProcedure, TParam parameters, Func<SqlMapper.GridReader, Task<TResult>> mapper, string connectionStringName = "DefaultConnection")
    {
        string connectionString = _config.GetConnectionString(connectionStringName) ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

        using IDbConnection connection = new SqlConnection(connectionString);

        using var gridReader = await connection.QueryMultipleAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

        return await mapper(gridReader);
    }
}
