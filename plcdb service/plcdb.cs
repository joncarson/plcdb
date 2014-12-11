using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using plcdb_lib.Models;
using System.Data.SqlClient;
using System.Threading;
using plcdb_lib.WCF;
using System.ServiceModel;
using System.IO;
using NLog;
using plcdb_lib.Logging;
using NLog.Config;

namespace plcdb_service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public partial class plcdb : ServiceBase, IServiceCommunicator
    {
        private Model ActiveModel = new Model();
        List<QueryWorker> ActiveWorkers = new List<QueryWorker>();
        List<Thread> ActiveThreads = new List<Thread>();
        Thread wcfThread;
        ServiceHost wcfHost;
        WcfLogTarget wcfLogger = new WcfLogTarget();

        public plcdb()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            
            //Thread.Sleep(9000);

            //Set up WCF logging
            LogManager.Configuration.AddTarget("wcf", wcfLogger);
            var rule = new LoggingRule("*", LogLevel.Trace, wcfLogger);
            LogManager.Configuration.LoggingRules.Add(rule);
            LogManager.ReconfigExistingLoggers();
            
            //Setup WCF communication host in its own thread
            wcfThread = new Thread(this.RunWcfServer);
            wcfThread.Start();

            //Begin using last-used Model
            if (Properties.Settings.Default.ActiveModelPath != null && File.Exists(Properties.Settings.Default.ActiveModelPath))
            {
                ActiveModel = ActiveModel.Open(Properties.Settings.Default.ActiveModelPath);
                CreateThreads();
            }
        }

        private void RunWcfServer()
        {
            using (wcfHost = new ServiceHost(this, new Uri[] {
                    new Uri("net.tcp://localhost/plcdb")
                  }))
            {
                wcfHost.AddServiceEndpoint(typeof(IServiceCommunicator),
                  new NetTcpBinding(),
                  "main");
                wcfHost.Open();
                while (true) Thread.Sleep(1);
            }
        }


        protected void CreateThreads()
        {
            StopThreads();
            ActiveThreads.Clear();
            ActiveWorkers.Clear();
            foreach (Model.QueriesRow Query in ActiveModel.Queries)
            {
                QueryWorker QueryWorker = new plcdb_service.QueryWorker(ActiveModel, Query);
                ActiveWorkers.Add(QueryWorker);
                Thread WorkerThread = new Thread(QueryWorker.DoWork);
                ActiveThreads.Add(WorkerThread);
                WorkerThread.Start();
            }
        }

        protected override void OnStop()
        {
            wcfThread.Abort();
            while (wcfThread.ThreadState != System.Threading.ThreadState.Aborted) Thread.Sleep(1);

           wcfHost.Close();
            
        }

        public void SetActiveModelPath(string ActiveModelPath)
        {
            Properties.Settings.Default.ActiveModelPath = ActiveModelPath;
            Properties.Settings.Default.Save();

            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += (s, e) =>
            {
                StopThreads();
                ActiveModel.Open(ActiveModelPath);
                CreateThreads();
            };

            bg.RunWorkerAsync();
        }

        private void StopThreads()
        {
            foreach (Thread thread in ActiveThreads)
            {
                thread.Abort();
            }
            while (ActiveThreads.Where(p => p.ThreadState != System.Threading.ThreadState.Aborted).Count() > 0) Thread.Sleep(1) ;

        }


        public List<ObjectStatus> GetQueriesStatus()
        {
            List<ObjectStatus> QueryStatuses = new List<ObjectStatus>();
            foreach (QueryWorker worker in ActiveWorkers)
            {
                QueryStatuses.Add(new ObjectStatus()
                {
                    PK = worker.ActiveQuery.PK,
                    Status = worker.Status
                });
            }
            return QueryStatuses;
        }

        public List<WcfEvent> GetLatestLogs(DateTime MinDate)
        {
            List<WcfEvent> events = wcfLogger.GetLatestLogs(MinDate); 
            return events;
        }
    }
}
