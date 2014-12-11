using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using plcdb.Helpers;
using plcdb_lib.Models;
using plcdb_lib.WCF;
using System.ServiceModel;
using System.Timers;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Threading;
using System.Data;
using plcdb_lib.Logging;
using NLog;
using System.Windows.Data;

namespace plcdb.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly BackgroundWorker bgQueryStatus = new BackgroundWorker();
        private readonly BackgroundWorker bgLogChecker = new BackgroundWorker();
        private Uri SERVICE_ENDPOINT = new Uri("net.tcp://localhost/plcdb/main");

        private object _serviceLogLock = new object();
        #region Properties

        #region ActiveModel
        private Model _activeModel = new Model();
        public Model ActiveModel
        {
            get { return _activeModel; }
            set
            {
                if (_activeModel != value)
                {
                    _activeModel = value;
                    RaisePropertyChanged(() => ActiveModel);
                }
            }
        }

        #endregion

        #region ActiveModelPath
        public String ActiveModelPath
        {
            get
            {
                return Properties.Settings.Default.LastOpenedFile;
                
            }
            set
            {
                if (Properties.Settings.Default.LastOpenedFile != value)
                {
                    Properties.Settings.Default.LastOpenedFile = value;
                    Properties.Settings.Default.Save();
                    RaisePropertyChanged(() => ActiveModelPath);
                }
            }
        }
        #endregion
 
        #region ServiceRunning
        private bool _serviceRunning;
        public bool ServiceRunning
        {
            get
            {
                return _serviceRunning;
            }
            set
            {
                if (_serviceRunning != value)
                {
                    _serviceRunning = value;
                    RaisePropertyChanged(() => ServiceRunning);
                }
            }
        }
        #endregion

        #region ServiceLogs
        private ObservableCollection<WcfEvent> _serviceLogs = new ObservableCollection<WcfEvent>();
        public ObservableCollection<WcfEvent> ServiceLogs
        {
            get { return _serviceLogs; }
            set
            {
                if (_serviceLogs != value)
                {
                    _serviceLogs = value;
                    RaisePropertyChanged(() => ServiceLogs);
                }
            }
        }

        #endregion

        private bool _activeModelChanged;
        public bool ActiveModelChanged
        {
            get
            {
                return _activeModelChanged;
            }
            set
            {
                _activeModelChanged = value;
                RaisePropertyChanged(() => ActiveModelChanged);
            }
        }

        public Model.DatabasesRow SelectedDatabase { get; set; }
        public Model.QueriesRow SelectedQuery { get; set; }
        public Model.ControllersRow SelectedController { get; set; }

        #endregion

        #region Commands

        public ICommand SetServiceModelCommand { get { return new DelegateCommand(OnSetServiceModelPath); } }
        public ICommand StartServiceCommand { get { return new DelegateCommand(OnStartService, () => { return !ServiceRunning; }); } }
        public ICommand StopServiceCommand { get { return new DelegateCommand(OnStopService, () => { return ServiceRunning; }); } }

        
        
        #endregion

        #region Ctor
        public MainWindowViewModel()
        {
            bgQueryStatus.DoWork += CheckQueryStatus;
            bgQueryStatus.RunWorkerCompleted += CheckQueryStatus_Callback;

            bgLogChecker.DoWork += CheckLogs;
            bgLogChecker.RunWorkerCompleted += CheckLogs_Callback;

            Timer serviceMonitor = new Timer(3000);
            serviceMonitor.Elapsed += serviceMonitor_Elapsed;
            serviceMonitor.Enabled = true;

            DispatcherTimer CanExecute = new DispatcherTimer();
            CanExecute.Interval = new TimeSpan(0, 0, 1);
            CanExecute.Tick += (s, e) => { CommandManager.InvalidateRequerySuggested(); };
            CanExecute.Start();

            BindingOperations.EnableCollectionSynchronization(ServiceLogs, _serviceLogLock);
        }

        private void CheckLogs_Callback(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                foreach (WcfEvent Event in (List<WcfEvent>)e.Result)
                {
                    ServiceLogs.Insert(0, Event);
                }
            }
        }

        private void CheckLogs(object sender, DoWorkEventArgs e)
        {
            ChannelFactory<IServiceCommunicator> channelFactory = null;
            try
            {
                channelFactory = new ChannelFactory<IServiceCommunicator>(
                    new NetTcpBinding() { MaxBufferSize = 2147483647, MaxReceivedMessageSize = 2147483647 }, 
                    new EndpointAddress(SERVICE_ENDPOINT));

                IServiceCommunicator Service =
                  channelFactory.CreateChannel();

                DateTime LatestDate;
                if (ServiceLogs.Count == 0)
                    LatestDate = DateTime.MinValue;
                else
                    LatestDate = ServiceLogs.Max(p => p.Occurred);

                e.Result = Service.GetLatestLogs(LatestDate);
                channelFactory.Close();
            }
            catch (Exception ex)
            {
                if (channelFactory != null)
                {
                    channelFactory.Abort();
                }
            }
        }

        
        void CheckQueryStatus_Callback(object sender, RunWorkerCompletedEventArgs e)
        {
            List<ObjectStatus> Statuses = e.Result as List<ObjectStatus>;
            foreach (Model.QueriesRow query in ActiveModel.Queries)
            {
                if (Statuses == null)
                {
                    query.Status = StatusEnum.Error.ToString();
                }
                else
                {
                    try
                    {
                        query.Status = Statuses.First(p => p.PK == query.PK).Status.ToString();
                    }
                    catch (Exception ex)
                    {
                        query.Status = StatusEnum.Error.ToString();
                        Log.Error(ex.Message);
                    }
                }
            }
        }

        void CheckQueryStatus(object sender, DoWorkEventArgs e)
        {
            ChannelFactory<IServiceCommunicator> channelFactory = null;
            try
            {
                channelFactory = new ChannelFactory<IServiceCommunicator>(new NetTcpBinding(), new EndpointAddress(SERVICE_ENDPOINT));
                IServiceCommunicator Service =
                  channelFactory.CreateChannel();

                e.Result = Service.GetQueriesStatus();
                channelFactory.Close();
                ServiceRunning = true;
            }
            catch (Exception ex)
            {
                ServiceRunning = false;
                if (channelFactory != null)
                {
                    channelFactory.Abort();
                }

            }
        }

        void serviceMonitor_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (! bgQueryStatus .IsBusy)
                bgQueryStatus.RunWorkerAsync();

            if (!bgLogChecker.IsBusy)
                bgLogChecker.RunWorkerAsync();
        }
        #endregion

        #region Command Handlers
        public void OnLoadModel()
        {
            ActiveModel = ActiveModel.Open(ActiveModelPath);
        }

        public void OnSaveModel()
        {
            ActiveModel.Save(ActiveModelPath);
        }

        public void OnSetServiceModelPath()
        {
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (s, e) => {
                ChannelFactory<IServiceCommunicator> channelFactory = null;
                try
                {
                    channelFactory = new ChannelFactory<IServiceCommunicator>(new NetTcpBinding(), new EndpointAddress(SERVICE_ENDPOINT));
                    IServiceCommunicator Service =
                      channelFactory.CreateChannel();

                    Service.SetActiveModelPath(ActiveModelPath);
                    channelFactory.Close();
                }
                catch (Exception ex)
                {
                    if (channelFactory != null)
                    {
                        channelFactory.Abort();
                    }

                }
            };

            bgWorker.RunWorkerAsync();

        }
        private void OnStartService()
        {
            ServiceController service = new ServiceController("plcdb");
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(5000);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }
        private void OnStopService()
        {
            ServiceController service = new ServiceController("plcdb");
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(5000);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }
        #endregion



        internal void DeleteRow(DataRow Row)
        {
            Row.Delete();
            ActiveModel.AcceptChanges();
        }
    }
}