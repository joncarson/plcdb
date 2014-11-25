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

        #region MyDateTime
        private Model _activeModel;
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

       
        #endregion

       
    }
}