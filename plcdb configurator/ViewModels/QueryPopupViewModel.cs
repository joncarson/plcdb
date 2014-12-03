using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using plcdb.Helpers;
using plcdb_lib.Models;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace plcdb.ViewModels
{
    public class QueryPopupViewModel : BaseViewModel
    {
         private MainWindowViewModel vm = App.Current.Resources["MainWindowViewModel"] as MainWindowViewModel;

        #region Properties

        #region AvailableDatabases
        private DataRowCollection _availableDatabases;
        public DataRowCollection AvailableDatabases
        {
            get
            {
                if (_availableDatabases == null)
                {
                    _availableDatabases = vm.ActiveModel.Databases.Rows;
                }
                return _availableDatabases;
            }
            set
            {
                _availableDatabases = value;
                RaisePropertyChanged(() => AvailableDatabases);
            }
        }
        #endregion

        #region AvailableQueryTypes
        private List<String> _availableQueryTypes;
        public List<String> AvailableQueryTypes
        {
            get
            {
                if (_availableQueryTypes == null)
                {
                    List<String> lst = new List<string>();
                    lst.Add("SELECT");
                    lst.Add("INSERT");
                    lst.Add("UPDATE");
                    lst.Add("DELETE");
                    _availableQueryTypes = lst;
                }
                return _availableQueryTypes;
            }
            set
            {
                _availableQueryTypes = value;
                RaisePropertyChanged(() => AvailableQueryTypes);
            }
        }
        #endregion

        #region CurrentQuery
        private plcdb_lib.Models.Model.QueriesRow _currentQuery;
        public plcdb_lib.Models.Model.QueriesRow CurrentQuery
        {
            get
            {
                return _currentQuery;
            }
            set
            {
                if (_currentQuery != value)
                {
                    _currentQuery = value;
                    RaisePropertyChanged(() => Name);
                }
                
                try
                {
                    Name = value.Name;
                    QueryType = value.QueryType;
                    QueryText = value.QueryText;
                    if (!value.IsDatabaseNull())
                        Database = value.DatabasesRow;
                    if (!value.IsTriggerTagNull())
                        TriggerTag = vm.ActiveModel.Tags.FindByPK(value.TriggerTag);
                    RefreshRate = value.RefreshRate;
                    MappingType = value.MappingType;
                    MaxRows = value.MaxRows;
                }
                catch (Exception e)
                {
                    Name = "";
                    QueryType = "";
                    QueryText = "";
                    RefreshRate = 5000;
                    MappingType = "";
                    MaxRows = 1;
                }
            }
        }
        #endregion

        #region Name
        public String Name
        {
            get { return CurrentQuery.Name; }
            set
            {
                if (CurrentQuery.Name != value)
                {
                    CurrentQuery.Name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }
        #endregion

        #region QueryType
        public string QueryType
        {
            get { return CurrentQuery.QueryType; }
            set
            {
                if (CurrentQuery.QueryType != null && CurrentQuery.QueryType != value)
                {
                    CurrentQuery.QueryType = value;
                    RaisePropertyChanged(() => QueryType);
                }
            }
        }
        #endregion
         
        #region QueryText
        public string QueryText
        {
            get { return CurrentQuery.QueryText; }
            set
            {
                if (CurrentQuery.QueryText != null && CurrentQuery.QueryText != value)
                {
                    CurrentQuery.QueryText = value;
                    RaisePropertyChanged(() => QueryText);
                }
            }
        }
        #endregion

        #region TriggerTag
        public Model.TagsRow TriggerTag
        {
            get 
            {
                if (CurrentQuery.IsTriggerTagNull())
                    return null;
                var vm = App.Current.Resources["MainWindowViewModel"] as MainWindowViewModel;
                return vm.ActiveModel.Tags.FindByPK(CurrentQuery.TriggerTag);
            }
            set
            {
                if (CurrentQuery.IsTriggerTagNull() || CurrentQuery.TriggerTag != value.PK)
                {
                    CurrentQuery.TriggerTag = value.PK;
                    RaisePropertyChanged(() => TriggerTag);
                }
            }
        }
        #endregion

        #region Database
        public Model.DatabasesRow Database
        {
            get 
            {
                if (CurrentQuery.IsDatabaseNull())
                    return null;
                return CurrentQuery.DatabasesRow;
            }
            set
            {
                if (CurrentQuery.IsDatabaseNull() || CurrentQuery.DatabasesRow != value)
                {
                    CurrentQuery.DatabasesRow = value;
                    CurrentQuery.Database = value.PK;
                    RaisePropertyChanged(() => Database);
                }
            }
        }
        #endregion

        #region RefreshRate
        public long RefreshRate
        {
            get { return CurrentQuery.RefreshRate; }
            set
            {
                if (CurrentQuery.RefreshRate != null && CurrentQuery.RefreshRate != value)
                {
                    CurrentQuery.RefreshRate = value;
                    RaisePropertyChanged(() => RefreshRate);
                }
            }
        }
        #endregion

        #region MappingType
        public string MappingType
        {
            get { return CurrentQuery.MappingType; }
            set
            {
                if (CurrentQuery.MappingType != null && CurrentQuery.MappingType != value)
                {
                    CurrentQuery.MappingType = value;
                    RaisePropertyChanged(() => MappingType);
                }
            }
        }
        #endregion

        #region MaxRows
        public long MaxRows
        {
            get { return CurrentQuery.MaxRows; }
            set
            {
                if (CurrentQuery.MaxRows != null && CurrentQuery.MaxRows != value)
                {
                    CurrentQuery.MaxRows = value;
                    RaisePropertyChanged(() => MaxRows);
                }
            }
        }
        #endregion

        #endregion


        #region Commands

        public ICommand SaveCommand { get { return new DelegateCommand(OnSave); } }
        public ICommand CancelCommand { get { return new DelegateCommand(OnCancel); } }

        
        #endregion

        #region Ctor
        public QueryPopupViewModel()
        {
            
        }
        
        #endregion

        #region Command Handlers

        private void OnSave()
        {
            CurrentQuery.AcceptChanges();
        }

        private void OnCancel()
        {
            CurrentQuery.RejectChanges();
        }
        #endregion

    }
}