using RYH.QueryPool.Core;
using System;

namespace ConsoleExample
{
    /// <summary>
    /// --------------------------------------------------------------------------------------------------------
    /// 
    /// Hello! Welcome!
    /// 
    /// --------------------------------------------------------------------------------------------------------
    /// 
    /// This is an example console project to show how to use QueryPool and QueryExecutor from ""
    /// This example shows a query been executed multiple times only for study purpose.
    /// 
    /// ------------------------------------------------------------------------------------------------------------
    ///
    /// Don't forget to configure a valid SQL connection string in variable _connectionString...
    /// ...and valid queries too!
    /// 
    /// ------------------------------------------------------------------------------------------------------------
    /// </summary>
    class Program
    {
        protected const string _connectionString = @"{connectionString}";

        static void Main(string[] args)
        {
            execute();
        }
        
        private static void execute()
        {
            int quantityParallel = 15; //Quantity of queries been executed in parallel
            int minSeconds = 5; //Min seconds for the delay between executions
            int maxSeconds = 15; //Max seconds for the delay between executions
            int recycleConnectionAfterNCalls = 10; //Recycle the SqlConnection after N calls for each query

            //Initial configuration
            QueryPool.Configure(_connectionString, recycleConnectionAfterNCalls);

            //Random generate for the delay between execution for the same query
            Random random = new Random();

            //Creates and starts each query generated
            for (int i = 0; i < quantityParallel; i++)
            {
                var item = new QueryPoolItem();
                item.Query = "SELECT TOP 1 * FROM TABLE";
                item.FrequencyInSeconds = random.Next(minSeconds, maxSeconds);
                item.QueryExecutedCallback = new Action<QueryResult>((queryResult) =>
                {
                    System.Console.WriteLine($"Item: {queryResult.Success} ({DateTime.Now})");
                });

                QueryPool.Add(item, startNow: true);
            }

            //If you want, you can add more items in pool while the pool is exexuting
            var newItem = new QueryPoolItem();
            newItem.Query = "SELECT TOP 1 * FROM TABLE";
            newItem.FrequencyInSeconds = 5;
            newItem.QueryExecutedCallback = new Action<QueryResult>((queryResult) =>
            {
                System.Console.WriteLine($"New Item: {queryResult.Success} ({DateTime.Now})");
            });

            //Add item to the existing pool and start it!
            QueryPool.Add(newItem, startNow: true);
        }

        private static void allFunctionsCommented()
        {
            //Initial configuration
            QueryPool.Configure("{connectionString}", recycleConnectionAfterNCalls: 15);

            //Starts all queries in the pool, or one in specific
            QueryPool.Start();
            QueryPool.Start(Guid.NewGuid()); //'Id' property of 'QueryPoolItem' class

            //Pauses all queries in the pool, or one in specific
            QueryPool.Pause();
            QueryPool.Pause(Guid.NewGuid()); //'Id' property of 'QueryPoolItem' class

            //Resets all queries in the pool, or one in specific
            QueryPool.Reset();
            QueryPool.Reset(Guid.NewGuid()); //'Id' property of 'QueryPoolItem' class

            //Kill the query pool and all instances inside of it. This is similar to a Dispose()
            QueryPool.Kill();
        }
    }
}
