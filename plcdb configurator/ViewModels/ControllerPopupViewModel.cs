using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using plcdb.Helpers;
using plcdb_lib.Models;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace plcdb.ViewModels
{
    public class ControllerPopupViewModel : BaseViewModel
    {
        #region Properties

        private IEnumerable<String> _availableControllerTypes;
        public IEnumerable<String> AvailableControllerTypes
        {
            get
            {
                if (_availableControllerTypes == null)
                {
                    _availableControllerTypes = new List<String>();
                    ((List<String>)_availableControllerTypes).Add("Simulator");
                }
                return _availableControllerTypes;
            }
            set
            {
                if (_availableControllerTypes != value)
                {
                    _availableControllerTypes = value;
                    RaisePropertyChanged(() => AvailableControllerTypes);
                }
            }
        }

        private plcdb_lib.Models.Model.ControllersRow _currentController;
        public plcdb_lib.Models.Model.ControllersRow CurrentController
        {
            get
            {
                return _currentController;
            }
            set
            {
                if (_currentController != value)
                {
                    _currentController = value;
                    RaisePropertyChanged(() => Name);
                }
                
                try
                {
                    Name = value.Name;
                    Address = value.Address;
                    ControllerType = value.Type;
                }
                catch (Exception e)
                {
                    Name = "";
                    Address = "";
                    ControllerType = "";
                }
            }
        }

        #region Name
        public String Name
        {
            get { return CurrentController.Name; }
            set
            {
                if (CurrentController.Name != value)
                {
                    CurrentController.Name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }
        #endregion

        #region Address
        public string Address
        {
            get { return CurrentController.Address; }
            set
            {
                if (CurrentController.Address != null && CurrentController.Address != value)
                {
                    CurrentController.Address = value;
                    RaisePropertyChanged(() => Address);
                }
            }
        }
        #endregion

        #region ControllerType
        public String ControllerType
        {
            get
            {
                return CurrentController.Type;
            }
            set
            {
                CurrentController.Type = value;
                RaisePropertyChanged(() => ControllerType);
            }
        }
        #endregion

        #endregion
        #region Commands

        public ICommand SaveCommand { get { return new DelegateCommand(OnSave); } }
        public ICommand CancelCommand { get { return new DelegateCommand(OnCancel); } }

        
        #endregion

        #region Ctor
        public ControllerPopupViewModel()
        {
            
        }
        
        #endregion

        #region Command Handlers

        private void OnSave()
        {
            CurrentController.AcceptChanges();
        }

        private void OnCancel()
        {
            CurrentController.RejectChanges();
        }
        #endregion

    }
}