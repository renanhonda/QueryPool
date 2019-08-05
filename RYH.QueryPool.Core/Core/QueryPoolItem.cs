using System;
using System.Collections.Generic;

namespace RYH.QueryPool.Core
{
    public class QueryPoolItem
    {
        public QueryPoolItem()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public string Query { get; set; }
        public int FrequencyInSeconds { get; set; }
        public List<SqlParameter> Parameters { get; set; }
        public Action<QueryResult> QueryExecutedCallback { get; set; }
    }
}
