using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace RYH.QueryPool.Core
{
    internal class SqlQueryToJson : IDisposable
    {
        private SqlConnection _conn;
        public SqlQueryToJson(string connectionString)
        {
            _conn = new SqlConnection(connectionString);
            _conn.Open();
        }

        public string ExecuteQuery(string query, List<SqlParameter> parameters)
        {
            try
            {
                SqlCommand command = _conn.CreateCommand();
                command.CommandText = query;

                if (parameters != null && parameters.Any())
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Name, param.Value);
                    }
                }
                
                SqlDataReader reader = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(reader);

                string json = JsonConvert.SerializeObject(table, Formatting.Indented);
                return json;                
            }
            catch (Exception ex)
            {
                throw new Exception("An error ocurred while executing query in database. For details see Inner Exception.", ex);
            }
        }

        public void Dispose()
        {
            try
            {
                if (_conn != null)
                {
                    _conn.Dispose();
                    _conn = null;
                }
            }
            catch { }

        }
    }
}
