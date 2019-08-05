using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;

namespace RYH.QueryPool.Core
{
    public class QueryExecutor : IDisposable
    {
        private SqlQueryToJson _sql = null;
        public QueryExecutor(string sqlConnectionString)
        {
            if (string.IsNullOrWhiteSpace(sqlConnectionString)) throw new Exception("The connection string must be informed.");
            _sql = new SqlQueryToJson(sqlConnectionString);
        }


        public QueryResult ExecuteSqlQuery(string query, List<SqlParameter> parameters)
        {
            try
            {
                var result = _sql.ExecuteQuery(query, parameters);                
                return new QueryResult() { JsonResult = result };
            }
            catch (Exception ex)
            {
                return getQueryResultCatch(ex);
            }
        }

        public async Task<QueryResult> ExecuteSqlQueryAsync(string query, List<SqlParameter> parameters)
        {
            var result = await Task.Run(() => this.ExecuteSqlQuery(query, parameters));
            return result;
        }

        public QueryResult ExecuteSqlQueryFromResource(string resourceFileNameWithoutExtension, string resourceKey, List<SqlParameter> parameters)
        {
            try
            {
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                return this.ExecuteSqlQueryFromResource(entryAssembly, resourceFileNameWithoutExtension, resourceKey, parameters);
            }
            catch (Exception ex)
            {
                return getQueryResultCatch(ex);
            }
        }

        public QueryResult ExecuteSqlQueryFromResource(Assembly assemblyToSearch, string resourceFileNameWithoutExtension, string resourceKey, List<SqlParameter> parameters)
        {
            try
            {
                if (assemblyToSearch == null) throw new Exception("A valid assembly must me informed.");
                string assemblyName = assemblyToSearch.GetName().Name;

                string resourceName = $"{assemblyName}.{resourceFileNameWithoutExtension}";
                ResourceManager resourceManager = new ResourceManager(resourceName, assemblyToSearch);

                var obj = resourceManager.GetObject(resourceKey);
                if (obj == null) throw new Exception($"The resource key '{resourceKey}' was not found.");

                string jsonObject = obj.ToString();
                if (string.IsNullOrWhiteSpace(jsonObject)) throw new Exception($"The resource '{resourceKey}' has no value informed.");

                QueryContract queryContract = QueryContract.ToQueryContract(jsonObject);
                if (!queryContract.Active) throw new Exception($"The resource '{resourceKey}' is not active.");

                return this.ExecuteSqlQuery(queryContract.Query, parameters);
            }
            catch (Exception ex)
            {
                return getQueryResultCatch(ex);
            }
        }

        public void Dispose()
        {
            if (_sql != null)
            {
                _sql.Dispose();
                _sql = null;
            }
        }


        private QueryResult getQueryResultCatch(Exception ex)
        {
            return new QueryResult() { MainException = new Exception("An error ocurred while processing. See details in InnerException.", ex) };
        }        
    }
}
