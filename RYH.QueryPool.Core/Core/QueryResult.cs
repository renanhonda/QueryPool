using Newtonsoft.Json;
using System;

namespace RYH.QueryPool.Core
{
    public class QueryResult
    {
        public bool Success
        {
            get
            {
                return this.MainException == null;
            }
        }

        public Exception MainException { get; set; }

        public string JsonResult { get; set; }
    }

    public class QueryResult<T> : QueryResult
    {
        public T ResultDeserialized
        {
            get
            {
                return JsonConvert.DeserializeObject<T>(this.JsonResult);
            }
        }
    }
}
