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
using plcdb_service.Licensing;
using plcdb_lib.constants;

namespace plcdb_service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    [LicenseProvider(typeof(ServiceLicenseProvider))]
    public partial class plcdb : ServiceBase, IServiceCommunicator
    {
        private Model ActiveModel = new Model();
        private List<QueryWorker> ActiveWorkers = new List<QueryWorker>();
        private List<Thread> ActiveThreads = new List<Thread>();
        private Thread wcfThread;
        private ServiceHost wcfHost;
        private WcfLogTarget wcfLogger = new WcfLogTarget();
        private System.Timers.Timer LicenseTimer = null;
        private bool DemoLicenseExpired = false;
        private DateTime StartTime;
        private Logger Log = LogManager.GetCurrentClassLogger();

        public plcdb()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            
            //Thread.Sleep(9000);
            StartTime = DateTime.Now;

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

            //Set up license timer
            if (License.IsDemo)
            {
                LicenseTimer = new System.Timers.Timer(CONSTANTS.DemoTimeout.TotalMilliseconds);
                LicenseTimer.Elapsed += LicenseTimer_Elapsed;
                LicenseTimer.AutoReset = false;
                LicenseTimer.Enabled = true;
                LicenseTimer.Start();
            }
                    
        }

        private void LicenseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StopThreads();
            ActiveThreads.Clear();
            ActiveWorkers.Clear();
            Log.Fatal("Demo session expired. Restart to continue using, or purchase license for continuous use.");
        }

        void RunWcfServer()
        {
            using (wcfHost = new ServiceHost(this, new Uri[] {
                    new Uri("net.tcp://" + System.Environment.MachineName + "/plcdb")
                  }))
            {
                wcfHost.AddServiceEndpoint(typeof(IServiceCommunicator),
                  new NetTcpBinding(SecurityMode.None),
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
            if (!DemoLicenseExpired)
            {
                foreach (Model.QueriesRow Query in ActiveModel.Queries)
                {
                    QueryWorker QueryWorker = new plcdb_service.QueryWorker(ActiveModel, Query);
                    ActiveWorkers.Add(QueryWorker);
                    Thread WorkerThread = new Thread(QueryWorker.DoWork);
                    ActiveThreads.Add(WorkerThread);
                    WorkerThread.Start();
                }
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
                    PK = worker.QueryPK,
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

        internal ServiceLicense License
        {
            get
            {
                return ServiceLicense.CreateLicense(Properties.Settings.Default.License);
            }
        }


        public DateTime GetStartTime()
        {
            if (License.IsDemo)
                return StartTime;
            return DateTime.MaxValue;
        }

        public void SetLicense(string license)
        {
            Properties.Settings.Default.License = license;
            if (!ServiceLicense.CreateLicense(license).IsDemo)
            {
                LicenseTimer.Stop();
            }
        }
    }
}
