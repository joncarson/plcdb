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
        #region Private variables
        private readonly BackgroundWorker bgQueryStatus = new BackgroundWorker();
        private readonly BackgroundWorker bgLogChecker = new BackgroundWorker();
        private Uri SERVICE_ENDPOINT = new Uri("net.tcp://localhost/plcdb/main");
        private object _serviceLogLock = new object();
        #endregion

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

        #region ServiceLogFilters
        public ICollectionView ServiceLogView { get; private set; }
        public Boolean LogFilterTrace { get; set; }
        public Boolean LogFilterDebug { get; set; }
        public Boolean LogFilterInfo  { get; set; }
        public Boolean LogFilterWarn  { get; set; }
        public Boolean LogFilterError { get; set; }
        public Boolean LogFilterFatal { get; set; }
        public Boolean PauseServiceLogging { get; set; }
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
        public ICommand ApplyLogFilterCommand { get { return new DelegateCommand(OnApplyLogFilter); } }

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

            //BindingOperations.EnableCollectionSynchronization(ServiceLogs, _serviceLogLock);

            ActiveModel.DatasetChanged += ActiveModel_DatasetChanged;

            ServiceLogView = CollectionViewSource.GetDefaultView(ServiceLogs);
            ServiceLogView.Filter = new Predicate<object>(ServiceLogFilter);
            LogFilterWarn = true;
            LogFilterError = true;
            LogFilterFatal = true;
        }
        #endregion

        void ActiveModel_DatasetChanged(object sender, EventArgs e)
        {
            ActiveModelChanged = true;
        }

        #region WCF Service Communication
        private void CheckLogs_Callback(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                foreach (WcfEvent Event in (List<WcfEvent>)e.Result)
                {
                    if (App.Current != null)
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            ServiceLogs.Insert(0, Event);
                        });
                    }
                }
                while (ServiceLogs.Count > 1000)
                {
                    if (App.Current != null)
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            ServiceLogs.RemoveAt(999);
                        });
                    }
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

            if (!bgLogChecker.IsBusy && !PauseServiceLogging)
                bgLogChecker.RunWorkerAsync();
        }
        #endregion

        #region Command Handlers
        public void OnLoadModel()
        {
            ActiveModel = ActiveModel.Open(ActiveModelPath);
            ActiveModelChanged = false;
        }
        public void OnSaveModel()
        {
            ActiveModel.Save(ActiveModelPath);
            ActiveModelChanged = false;
        }
        public void OnSetServiceModelPath()
        {
            OnSaveModel();
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
        private void OnApplyLogFilter()
        {
            ServiceLogView.Refresh();
        }
        internal void DeleteRow(DataRow Row)
        {
            Row.Delete();
            ActiveModel.AcceptChanges();
        }
        private bool ServiceLogFilter(object item)
        {
            try
            {
                bool keepItem = true;
                WcfEvent row = (WcfEvent)item;
                if (row.LogLevel == "Trace" && !LogFilterTrace)
                    keepItem = false;
                else if (row.LogLevel == "Debug" && !LogFilterDebug)
                    keepItem = false;
                else if (row.LogLevel == "Info" && !LogFilterInfo)
                    keepItem = false;
                else if (row.LogLevel == "Warn" && !LogFilterWarn)
                    keepItem = false;
                else if (row.LogLevel == "Error" && !LogFilterError)
                    keepItem = false;
                else if (row.LogLevel == "Fatal" && !LogFilterFatal)
                    keepItem = false;

                if (!ActiveModel.Queries.First(p => p.PK == row.Query).Logged)
                    keepItem = false;

                return keepItem;
            }
            catch
            {
                return true;
            }
        }
        #endregion



        


    }
}