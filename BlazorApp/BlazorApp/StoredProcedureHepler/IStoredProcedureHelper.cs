using Dapper;
using Microsoft.Data.SqlClient;
namespace BlazorApp.StoredProcedureHepler;

public interface IStoredProcedureHelper : IDisposable
{
    // Phương thức thực hiện procedure không trả về kết quả (ví dụ: Insert/Update/Delete)
    int ExecuteNonQuery(string procedureName, object parameters = null);
    // Trả về 1 List<T>
    Task<List<T>> ExecuteQuery<T>(string procedureName, object parameters = null);
    //Trả về 1 object <T>
    Task<object> ExecuteScalar(string procedureName, object parameters = null);       
}
