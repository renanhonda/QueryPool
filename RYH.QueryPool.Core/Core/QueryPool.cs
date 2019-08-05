using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace RYH.QueryPool.Core
{
    public static class QueryPool
    {
        private static List<Aux> _lstAux = null;
        private static bool _isConfigured = false;
        private static string _sqlConnectionString = string.Empty;
        private static int _recycleConnectionAfterNCalls = 0;

        public static void Configure(string sqlConnectionString, int recycleConnectionAfterNCalls = 15)
        {
            if (_lstAux != null) throw new Exception("The pool is already configured. To configure a new pool, call the 'Kill()' function before.");
            if (string.IsNullOrWhiteSpace(sqlConnectionString)) throw new Exception("The connection string must be informed.");

            _lstAux = new List<Aux>();
            _sqlConnectionString = sqlConnectionString;
            _recycleConnectionAfterNCalls = recycleConnectionAfterNCalls;
            _isConfigured = true;
        }

        public static void Add(QueryPoolItem item, bool startNow = false)
        {
            if (item == null) throw new Exception("The query pool item must be informed");
            if (item.FrequencyInSeconds < 5) throw new Exception("The frequencies must be higher than 5 seconds.");
            if (string.IsNullOrWhiteSpace(item.Query)) throw new Exception("The queries must be informed.");
            if (item.QueryExecutedCallback == null) throw new Exception("All queries callbacks must be informed.");

            Timer timer = new Timer
            {
                Enabled = true,
                Interval = 1000
            };

            Aux aux = new Aux()
            {
                Timer = timer,
                QueryPoolItem = item,
                Executor = getNewExecutor()
            };

            timer.Elapsed += (sender, args) => timerElapsed(sender, aux);

            _lstAux.Add(aux);
            if (startNow) start(aux);
        }

        public static void Start()
        {
            foreach (var timer in _lstAux)
            {
                start(timer);
            }
        }

        public static void Start(Guid idQueryPoolItem)
        {
            var timer = _lstAux?.FirstOrDefault(t => t.QueryPoolItem.Id == idQueryPoolItem);
            start(timer);
        }

        public static void Pause()
        {
            foreach (var timer in _lstAux)
            {
                stop(timer);
            }
        }

        public static void Pause(Guid idQueryPoolItem)
        {
            var timer = _lstAux?.FirstOrDefault(t => t.QueryPoolItem.Id == idQueryPoolItem);
            stop(timer);
        }

        public static void Reset()
        {
            foreach (var timer in _lstAux)
            {
                stop(timer);
                start(timer);
            }
        }

        public static void Reset(Guid idQueryPoolItem)
        {
            var timer = _lstAux?.FirstOrDefault(t => t.QueryPoolItem.Id == idQueryPoolItem);
            if (timer != null)
            {
                stop(timer);
                start(timer);
            }
        }

        public static void Kill()
        {
            try
            {
                if (_lstAux != null)
                {
                    Pause();
                    foreach (var timer in _lstAux)
                    {
                        timer.Timer.Close();
                        timer.Timer.Dispose();
                        timer.Timer = null;

                        timer.QueryPoolItem = null;

                        timer.Executor.Dispose();
                        timer.Executor = null;
                    }
                }
                _lstAux = null;
                _isConfigured = false;
                _sqlConnectionString = string.Empty;
                _recycleConnectionAfterNCalls = 0;

                GC.Collect();
            }
            catch { }
        }



        private static void start(Aux timer)
        {
            if (!_isConfigured) throw new Exception("The 'Configure()' function must be called before starting the pool.");
            if (timer != null && !timer.Timer.Enabled)
            {
                timer.Timer.Start();
            }
        }

        private static void stop(Aux timer)
        {
            if (!_isConfigured) return;
            if (timer != null && timer.Timer.Enabled)
            {
                timer.Timer.Stop();
            }
        }

        private static void timerElapsed(object sender, Aux aux)
        {
            aux.Timer.Stop();

            aux.ExecutionCounter += 1;
            if(aux.ExecutionCounter >= _recycleConnectionAfterNCalls)
            {
                aux.Executor.Dispose();
                aux.Executor = null;
                aux.Executor = getNewExecutor();
            }

            QueryResult queryResult = aux.Executor.ExecuteSqlQuery(aux.QueryPoolItem.Query, aux.QueryPoolItem.Parameters);

            var newFrequency = aux.QueryPoolItem.FrequencyInSeconds * 1000;
            if (aux.Timer.Interval != newFrequency) aux.Timer.Interval = newFrequency;

            aux.QueryPoolItem.QueryExecutedCallback(queryResult);
            aux.Timer.Start();
        }

        private static QueryExecutor getNewExecutor() => new QueryExecutor(_sqlConnectionString);

        private class Aux
        {
            public Timer Timer { get; set; }
            public QueryPoolItem QueryPoolItem { get; set; }
            public QueryExecutor Executor { get; set; }
            public int ExecutionCounter { get; set; }
        }
    }


}
