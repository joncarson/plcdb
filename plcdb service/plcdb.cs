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

namespace plcdb_service
{
    public partial class plcdb : ServiceBase, IServiceCommunicator
    {
        private Model ActiveModel = new Model();
        List<QueryWorker> ActiveWorkers = new List<QueryWorker>();
        List<Thread> ActiveThreads = new List<Thread>();

        public plcdb()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //Load model
            ActiveModel.Open(Properties.Settings.Default.ActiveModelPath);

            //Setup WCF communication host in its own thread
            Thread wcfThread = new Thread(this.RunWcfServer);
            wcfThread.Start();

            //Begin using last-used Model
            if (Properties.Settings.Default.ActiveModelPath != null && File.Exists(Properties.Settings.Default.ActiveModelPath))
            {
                CreateThreads();
            }
        }

        private void RunWcfServer()
        {
            using (ServiceHost host = new ServiceHost(typeof(plcdb), new Uri[] {
                    new Uri("net.pipe://localhost")
                  }))
            {
                host.AddServiceEndpoint(typeof(IServiceCommunicator),
                  new NetNamedPipeBinding(),
                  "plcdb");
                host.Open();
                while (true) Thread.Sleep(1);
            }
        }

        protected void CreateThreads()
        {
            ActiveThreads.Clear();
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

        }

        public void SetActiveModelPath(string ActiveModelPath)
        {
            Properties.Settings.Default.ActiveModelPath = ActiveModelPath;
            Properties.Settings.Default.Save();

            foreach (Thread queryThread in ActiveThreads)
            {
                queryThread.Abort();
            }

            ActiveModel.Open(ActiveModelPath);
            CreateThreads();
        }
    }
}
