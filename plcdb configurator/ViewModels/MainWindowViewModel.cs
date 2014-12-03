using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using plcdb.Helpers;
using plcdb_lib.Models;

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

        #endregion

        #region Commands

        //public ICommand RefreshDateCommand { get { return new DelegateCommand(OnRefreshDate); } }
        
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

        #endregion

       
    }
}