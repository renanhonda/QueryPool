using Newtonsoft.Json;
using System;

namespace RYH.QueryPool.Core
{
    internal class QueryContract
    {
        public string Title { get; set; }
        public int FrequencyInSeconds { get; set; }
        public bool Active { get; set; }
        public string Query { get; set; }

        public static QueryContract ToQueryContract(string json)
        {
            try
            {
                var deserialized = JsonConvert.DeserializeObject<QueryContract>(json);
                return deserialized;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error ocurred while deserializing JSON object: {json}", ex);
            }
        }
    }
}
