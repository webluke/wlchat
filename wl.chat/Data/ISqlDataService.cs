using Dapper;

namespace wl.chat.Data
{
    public interface ISqlDataService
    {
        Task<IEnumerable<TParam>> LoadData<TParam, TResult>(string storedProcedure, TResult parameters, string connectionStringName = "DefaultConnection");
        Task<TResult> LoadMultipleData<TParam, TResult>(string storedProcedure, TParam parameters, Func<SqlMapper.GridReader, Task<TResult>> mapper, string connectionStringName = "DefaultConnection");
        Task SaveData<TParam>(string storedProcedure, TParam parameters, string connectionStringName = "DefaultConnection");
    }
}