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
        private String _activeModelPath;
        public String ActiveModelPath
        {
            get { return _activeModelPath; }
            set
            {
                if (_activeModelPath != value)
                {
                    _activeModelPath = value;
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