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
using plcdb_lib.constants;
using System.Net;
using plcdb_lib.HelperFunctions;

namespace plcdb.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Private variables
        private readonly BackgroundWorker bgQueryStatus = new BackgroundWorker();
        private readonly BackgroundWorker bgLogChecker = new BackgroundWorker();
        private readonly BackgroundWorker bgLicenseChecker = new BackgroundWorker();
        private List<WcfEvent> eventLog = new List<WcfEvent>();
        
        private object _serviceLogLock = new object();
        private Uri ServicePath
        {
            get
            {
                return new Uri("net.tcp://" + ServiceHostComputer + "/plcdb/main");
            }
        }

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

        #region LicenseKey
        public String LicenseKey
        {
            get
            {
                return Properties.Settings.Default.LicenseKey;

            }
            set
            {
                if (Properties.Settings.Default.LicenseKey != value)
                {
                    Properties.Settings.Default.LicenseKey = value;
                    Properties.Settings.Default.Save();
                    RaisePropertyChanged(() => LicenseKey);
                }
            }
        }
        #endregion

        #region PurchaseKey
        public String PurchaseKey
        {
            get
            {
                return Properties.Settings.Default.PurchaseKey;

            }
            set
            {
                if (Properties.Settings.Default.PurchaseKey != value)
                {
                    Properties.Settings.Default.PurchaseKey = value;
                    Properties.Settings.Default.Save();
                    RaisePropertyChanged(() => PurchaseKey);
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

        #region IsDemo
        private bool _isDemo;
        public bool IsDemo
        {
            get
            {
                return _isDemo;
            }
            set
            {
                if (_isDemo != value)
                {
                    _isDemo = value;
                    RaisePropertyChanged(() => IsDemo);
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

        public int MaxRowsInLog
        {
            get
            {
                return Properties.Settings.Default.MaxLogLength;
            }
            set
            {
                Properties.Settings.Default.MaxLogLength = value;
                Properties.Settings.Default.Save();
            }
        }
        public String ServiceHostComputer
        {
            get
            {
                return Properties.Settings.Default.ServiceHost;
            }
            set
            {
                Properties.Settings.Default.ServiceHost = value;
                Properties.Settings.Default.Save();
            }
        }
        #region Selected Rows
        private Model.DatabasesRow _selectedDatabase;
        public Model.DatabasesRow SelectedDatabase
        {
            get
            {
                return _selectedDatabase;
            }
            set
            {
                if (value != _selectedDatabase)
                {
                    _selectedDatabase = value;
                    RaisePropertyChanged(() => SelectedDatabase);
                }
            }
        }

        private Model.ControllersRow _selectedController;
        public Model.ControllersRow SelectedController
        {
            get
            {
                return _selectedController;
            }
            set
            {
                if (value != _selectedController)
                {
                    _selectedController = value;
                    RaisePropertyChanged(() => SelectedController);
                }
            }
        }

        private Model.QueriesRow _selectedQuery;
        public Model.QueriesRow SelectedQuery
        {
            get
            {
                return _selectedQuery;
            }
            set
            {
                if (value != _selectedQuery)
                {
                    _selectedQuery = value;
                    RaisePropertyChanged(() => SelectedQuery);
                }
            }
        }
        #endregion

        private DateTime _licenseStartTime;
        public DateTime LicenseStartTime
        {
            get
            {
                return _licenseStartTime;
            }
            set
            {
                if (_licenseStartTime != value)
                {
                    _licenseStartTime = value;
                }
                RaisePropertyChanged(() => LicenseStartTime);
            }
        }
        #endregion

        #region Commands

        public ICommand SetServiceModelCommand { get { return new DelegateCommand(OnSetServiceModelPath); } }
        public ICommand StartServiceCommand { get { return new DelegateCommand(OnStartService, () => { return !ServiceRunning; }); } }
        public ICommand StopServiceCommand { get { return new DelegateCommand(OnStopService, () => { return ServiceRunning; }); } }
        public ICommand ApplyLogFilterCommand { get { return new DelegateCommand(OnApplyLogFilter); } }
        public ICommand CopyDatabaseCommand { get { return new DelegateCommand(OnCopyDatabase); } }
        public ICommand CopyControllerCommand { get { return new DelegateCommand(OnCopyController); } }
        public ICommand CopyQueryCommand { get { return new DelegateCommand(OnCopyQuery); } }
        public ICommand ValidateLicenseFromUrlCommand { get { return new DelegateCommand(OnValidateLicenseFromUrl); } }
        

        #endregion

        #region Ctor
        public MainWindowViewModel()
        {
            bgQueryStatus.DoWork += CheckQueryStatus;
            bgQueryStatus.RunWorkerCompleted += CheckQueryStatus_Callback;

            bgLogChecker.DoWork += CheckLogs;
            bgLogChecker.RunWorkerCompleted += CheckLogs_Callback;

            bgLicenseChecker.DoWork += CheckLicenseStatus;
            bgLicenseChecker.RunWorkerCompleted += CheckLicenseStatus_Callback;

            Timer serviceMonitor = new Timer(3000);
            serviceMonitor.Elapsed += serviceMonitor_Elapsed;
            serviceMonitor.Enabled = true;

            DispatcherTimer CanExecute = new DispatcherTimer();
            CanExecute.Interval = new TimeSpan(0, 0, 1);
            CanExecute.Tick += (s, e) => { CommandManager.InvalidateRequerySuggested(); };
            CanExecute.Start();

            DispatcherTimer CheckLogsThread = new DispatcherTimer();
            CheckLogsThread.Interval = new TimeSpan(0, 0, 3);
            CheckLogsThread.Tick += CheckLogs_Tick;
            CheckLogsThread.Start();

            //BindingOperations.EnableCollectionSynchronization(ServiceLogs, _serviceLogLock);

            ActiveModel.DatasetChanged += ActiveModel_DatasetChanged;

            ServiceLogView = CollectionViewSource.GetDefaultView(ServiceLogs);
            ServiceLogView.Filter = new Predicate<object>(ServiceLogFilter);
            LogFilterWarn = true;
            LogFilterError = true;
            LogFilterFatal = true;
        }

        private void CheckLogs_Tick(object sender, EventArgs e)
        {
            DateTime LatestDate = DateTime.MinValue;
            if (ServiceLogs.Count > 0)
                LatestDate = ServiceLogs.Max(p => p.Occurred);

            foreach (WcfEvent Event in eventLog.Where(p => p.Occurred > LatestDate))
            {
                ServiceLogs.Insert(0, Event);
            }
            while (ServiceLogs.Count > MaxRowsInLog)
            {
                try
                {
                    ServiceLogs.RemoveAt(MaxRowsInLog - 1);
                }
                catch (ArgumentOutOfRangeException ex) //sometimes threadsafe invoke messes up and throws exception. Ignore this one type of exception
                {
                }
            }
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
                eventLog = (List<WcfEvent>)e.Result;
            }
        }
        private void CheckLogs(object sender, DoWorkEventArgs e)
        {
            ChannelFactory<IServiceCommunicator> channelFactory = null;
            try
            {
                NetTcpBinding ServiceBinding = new NetTcpBinding();
                ServiceBinding.Security.Mode = SecurityMode.None;
                ServiceBinding.MaxBufferSize = 2147483647;
                ServiceBinding.MaxReceivedMessageSize = 2147483647;
                channelFactory = new ChannelFactory<IServiceCommunicator>(ServiceBinding, new EndpointAddress(ServicePath));
                
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
            foreach (Model.QueriesRow query in (Model.QueriesDataTable)ActiveModel.Queries)
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
                NetTcpBinding ServiceBinding = new NetTcpBinding();
                ServiceBinding.Security.Mode = SecurityMode.None;
                channelFactory = new ChannelFactory<IServiceCommunicator>(ServiceBinding, new EndpointAddress(ServicePath));
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
        private void CheckLicenseStatus_Callback(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                LicenseStartTime = (DateTime)e.Result;
                IsDemo = LicenseStartTime != DateTime.MaxValue;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                LicenseStartTime = DateTime.MaxValue;
            }
        }

        private void CheckLicenseStatus(object sender, DoWorkEventArgs e)
        {
            ChannelFactory<IServiceCommunicator> channelFactory = null;
            try
            {
                NetTcpBinding ServiceBinding = new NetTcpBinding();
                ServiceBinding.Security.Mode = SecurityMode.None;
                channelFactory = new ChannelFactory<IServiceCommunicator>(ServiceBinding, new EndpointAddress(ServicePath));
                IServiceCommunicator Service =
                  channelFactory.CreateChannel();

                e.Result = Service.GetStartTime();
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

            if (!bgLicenseChecker .IsBusy)
                bgLicenseChecker.RunWorkerAsync();
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
                    NetTcpBinding ServiceBinding = new NetTcpBinding();
                    ServiceBinding.Security.Mode = SecurityMode.None;
                    channelFactory = new ChannelFactory<IServiceCommunicator>(ServiceBinding, new EndpointAddress(ServicePath));
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
            BackgroundWorker bgStartThread = new BackgroundWorker();
            bgStartThread.DoWork += (s, e) =>
                {
                    ServiceController service = new ServiceController(CONSTANTS.ServiceName);
                    try
                    {
                        TimeSpan timeout = TimeSpan.FromMilliseconds(10000);

                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                    }
                };
            bgStartThread.RunWorkerAsync();
        }
        private void OnStopService()
        {
            ServiceController service = new ServiceController(CONSTANTS.ServiceName);
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
        private void OnCopyDatabase()
        {
            Model.DatabasesRow NewRow = ActiveModel.Databases.NewDatabasesRow();
            foreach (DataColumn col in ActiveModel.Databases.Columns)
            {
                if (col.ColumnName != "PK")
                {
                    NewRow[col.ColumnName] = SelectedDatabase[col.ColumnName];
                }
            }
            ActiveModel.Databases.AddDatabasesRow(NewRow);
        }
        private void OnCopyController()
        {
            Model.ControllersRow NewRow = ActiveModel.Controllers.NewControllersRow();
            foreach (DataColumn col in ActiveModel.Controllers.Columns)
            {
                if (col.ColumnName != "PK")
                {
                    NewRow[col.ColumnName] = SelectedController[col.ColumnName];
                }
            }
            ActiveModel.Controllers.AddControllersRow(NewRow);
        }
        private void OnCopyQuery()
        {
            Model.QueriesRow NewRow = ActiveModel.Queries.NewQueriesRow();
            foreach (DataColumn col in ActiveModel.Queries.Columns)
            {
                if (col.ColumnName != "PK")
                {
                    NewRow[col.ColumnName] = SelectedQuery[col.ColumnName];
                }
            }
            ActiveModel.Queries.AddQueriesRow(NewRow);

            foreach (Model.QueryTagMappingsRow Row in SelectedQuery.GetQueryTagMappingsRows())
            {
                Model.QueryTagMappingsRow NewTagMappingRow = ActiveModel.QueryTagMappings.NewQueryTagMappingsRow();
                foreach (DataColumn col in ActiveModel.QueryTagMappings.Columns)
                {
                    if (col.ColumnName == "Query")
                    {
                        NewTagMappingRow[col.ColumnName] = NewRow.PK;
                    }
                    else if (col.ColumnName != "PK")
                    {
                        NewTagMappingRow[col.ColumnName] = Row[col.ColumnName];
                    }
                }
                ActiveModel.QueryTagMappings.AddQueryTagMappingsRow(NewTagMappingRow);
            }
            ActiveModel.Queries.AcceptChanges();
            ActiveModel.QueryTagMappings.AcceptChanges();
            
        }

        private void OnValidateLicenseFromUrl()
        {
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (s, e) =>
            {
                ChannelFactory<IServiceCommunicator> channelFactory = null;
                try
                {
                    NetTcpBinding ServiceBinding = new NetTcpBinding();
                    ServiceBinding.Security.Mode = SecurityMode.None;
                    channelFactory = new ChannelFactory<IServiceCommunicator>(ServiceBinding, new EndpointAddress(ServicePath));
                    IServiceCommunicator Service = channelFactory.CreateChannel();
                    String UniqueHW = Service.GetUniqueHWID();
                    
                    using (WebClient client = new WebClient())
                    {
                        LicenseKey = client.DownloadString("http://joncarsondev.appspot.com/plc2sql?pk=" + PurchaseKey + "&hid=" + LicenseHelper.Unique_HW_ID());
                        Service.SetLicense(PurchaseKey, LicenseKey);
                        if (LicenseHelper.IsLicenseValid(LicenseKey, PurchaseKey, UniqueHW))
                        {
                            IsDemo = false;
                        }
                        else
                        {
                            IsDemo = true;
                        }
                    }
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
        private bool ServiceLogFilter(object item)
        {
            try
            {
                WcfEvent row = (WcfEvent)item;
                if (row.LogLevel == "Trace" && !LogFilterTrace)
                    return false;
                else if (row.LogLevel == "Debug" && !LogFilterDebug)
                    return false;
                else if (row.LogLevel == "Info" && !LogFilterInfo)
                    return false;
                else if (row.LogLevel == "Warn" && !LogFilterWarn)
                    return false;
                else if (row.LogLevel == "Error" && !LogFilterError)
                    return false;
                else if (row.LogLevel == "Fatal" && !LogFilterFatal)
                    return false;

                Model.QueriesRow Row = ActiveModel.Queries.FindByPK(row.Query);
                if (Row != null && !Row.Logged)
                    return false;
                
                return true;
            }
            catch
            {
                return true;
            }
        }
        #endregion
    }
}