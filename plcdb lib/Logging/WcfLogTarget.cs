using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Runtime.CompilerServices;
using NLog.Targets;
using System.Threading;
using System.ServiceModel;
using plcdb_lib.WCF;

namespace plcdb_lib.Logging
{
    [Target("WcfTarget")]
    public sealed class WcfLogTarget : TargetWithLayout
    {
        private static List<WcfEvent> _latestLogs = new List<WcfEvent>();
        
        public WcfLogTarget()
        {
            
        }

        protected override void Write(LogEventInfo logEvent)
        {
            int Query = 0;
            if (logEvent.Properties.ContainsKey("Query"))
                Query = int.Parse(logEvent.Properties["Query"].ToString());
            lock (_latestLogs)
            {
                _latestLogs.Add(new WcfEvent()
                    {
                        LogLevel = logEvent.Level.ToString(),
                        Message = logEvent.FormattedMessage,
                        Name = logEvent.LoggerName,
                        Occurred = logEvent.TimeStamp,
                        Query = Query,
                        StackTrace = logEvent.StackTrace == null ? null : logEvent.StackTrace.ToString()
                    });
                if (_latestLogs.Count > 1000)
                    _latestLogs.RemoveAt(0);
            }
        }

        public List<WcfEvent> GetLatestLogs(DateTime MinDate)
        {
            lock (_latestLogs)
                return _latestLogs.Where(p => p.Occurred > MinDate).ToList();
        }

        
        
    }
}
