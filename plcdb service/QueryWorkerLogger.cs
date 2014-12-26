using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace plcdb_service
{
    class QueryWorkerLogger : Logger
    {
        public void Trace(QueryWorker Worker, object Message)
        {
            if (Message is Exception)
                base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Trace, Worker.QueryPK, (Exception)Message));
            else if (Message is String)
                base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Trace, Worker.QueryPK, (String)Message));
            else
                base.Trace(Message);
        }

        public void Info(QueryWorker Worker, object Message)
        {
            if (Message is Exception)
                base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Info, Worker.QueryPK, (Exception)Message));
            else if (Message is String)
                base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Info, Worker.QueryPK, (String)Message));
            else
                base.Info(Message);
        }

        public void Debug(QueryWorker Worker, object Message)
        {
            if (Message is Exception)
                base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Debug, Worker.QueryPK, (Exception)Message));
            else if (Message is String)
                base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Debug, Worker.QueryPK, (String)Message));
            else
                base.Debug(Message);
        }

        public void Warn(QueryWorker Worker, object Message)
        {
            if (Message is Exception)
                base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Warn, Worker.QueryPK, (Exception)Message));
            else if (Message is String)
                base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Warn, Worker.QueryPK, (String)Message));
            else
                base.Warn(Message);
        }

        public void Error(QueryWorker Worker, object Message)
        {
            if (Message is Exception)
                base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Error, Worker.QueryPK, (Exception)Message));
            else if (Message is String)
                base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Error, Worker.QueryPK, (String)Message));
            else
                base.Error(Message);
        }


        public void Fatal(QueryWorker Worker, object Message)
        {
            if (Message is Exception)
                base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Fatal, Worker.QueryPK, (Exception)Message));
            else if (Message is String)
                base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Fatal, Worker.QueryPK, (String)Message));
            else
                base.Fatal(Message);
        }

        private LogEventInfo GetLogEventInfo(LogLevel level, long QueryPK, Exception ex)
        {
            LogEventInfo LogEvent = new LogEventInfo()
            {
                Message = ex.Message,
                Level = level,
                TimeStamp = DateTime.Now,
                LoggerName = "Query Worker",
            };
            LogEvent.Properties["Query"] = QueryPK;
            return LogEvent;
        }

        private LogEventInfo GetLogEventInfo(LogLevel level, long QueryPK, String msg)
        {
            LogEventInfo LogEvent = new LogEventInfo()
            {
                Message = msg,
                Level = level,
                TimeStamp = DateTime.Now,
                LoggerName = "Query Worker",
            };
            LogEvent.Properties["Query"] = QueryPK;
            return LogEvent;
        }

        public void TraceEnterFunction(QueryWorker worker, string memberName = "")
        {
            base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Trace, worker.QueryPK, "Entering function: " + memberName));
        }

        public void TraceExitFunction(QueryWorker worker, string memberName = "")
        {
            base.Log(typeof(QueryWorkerLogger), GetLogEventInfo(LogLevel.Trace, worker.QueryPK, "Exiting function: " + memberName));
        }
    }
}
