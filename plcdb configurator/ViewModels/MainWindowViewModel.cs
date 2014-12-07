using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using plcdb.Helpers;
using plcdb_lib.Models;
using plcdb_lib.WCF;
using System.ServiceModel;

namespace plcdb.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
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

        #region ServiceCommunicator
        private IServiceCommunicator _serviceCommunicator;
        public IServiceCommunicator ServiceCommunicator
        {
            get
            {
                if (_serviceCommunicator == null)
                {
                    ChannelFactory<IServiceCommunicator> pipeFactory =
                    new ChannelFactory<IServiceCommunicator>(
                      new NetNamedPipeBinding(),
                      new EndpointAddress(
                        "net.pipe://localhost/plcdb"));

                  _serviceCommunicator =
                    pipeFactory.CreateChannel();
                }
                return _serviceCommunicator;
            }
            set
            {
                if (_serviceCommunicator != value)
                {
                    _serviceCommunicator = value;
                    RaisePropertyChanged(() => ServiceCommunicator);
                }
            }
        }
        #endregion
        #endregion

        #region Commands

        public ICommand SetServiceModelCommand { get { return new DelegateCommand(OnSetServiceModelPath); } }
        
        #endregion

        #region Ctor
        public MainWindowViewModel()
        {
            
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
            try
            {
                ServiceCommunicator.SetActiveModelPath(ActiveModelPath);
            }
            catch (EndpointNotFoundException e)
            {
                Log.Error("Attempted to set serice model path but WCF server was not found");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        #endregion

       
    }
}