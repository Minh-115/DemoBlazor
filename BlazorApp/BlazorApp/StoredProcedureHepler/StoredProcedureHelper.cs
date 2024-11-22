using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace BlazorApp.StoredProcedureHepler
{
    public class StoredProcedureHelper : IStoredProcedureHelper
    {
        private readonly string _connectionString;

        public StoredProcedureHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public int ExecuteNonQuery(string procedureName, object parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Execute(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public List<T> ExecuteQuery<T>(string procedureName, object parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<T>(procedureName, parameters, commandType: CommandType.StoredProcedure).ToList();
            }
        }

        public object ExecuteScalar(string procedureName, object parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.ExecuteScalar(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public void Dispose()
        {            
        }

    }
}
