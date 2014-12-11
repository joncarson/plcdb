using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using plcdb.Helpers;
using plcdb_lib.Models;
using System.Data.SqlClient;
using System.Data;
using System.Linq;

namespace plcdb.ViewModels
{
    public class TagBrowserViewModel : BaseViewModel
    {
        protected Model ActiveModel = (App.Current.Resources["MainWindowViewModel"] as MainWindowViewModel).ActiveModel;
        #region Properties

        #region AvailableControllers
        private DataRowCollection _availableControllers;
        public DataRowCollection AvailableControllers
        {
            get
            {
                if (_availableControllers == null)
                {
                    _availableControllers = ActiveModel.Controllers.Rows;
                }
                return _availableControllers;
            }
            set
            {
                _availableControllers = value;
                RaisePropertyChanged(() => AvailableControllers);
            }
        }
        #endregion

        #region AvailableTags
        private EnumerableRowCollection _availableTags;
        public EnumerableRowCollection AvailableTags
        {
            get
            {
                if (CurrentController == null)
                {
                    return null;
                }
                if (_availableTags == null)
                {
                    _availableTags = ActiveModel.Tags.Where(p => p.Controller == CurrentController.PK);
                }
                return _availableTags;
            }
            set
            {
                _availableTags = value;
                RaisePropertyChanged(() => AvailableTags);
            }
        }
        #endregion

        private plcdb_lib.Models.Model.TagsRow _currentTag;
        public plcdb_lib.Models.Model.TagsRow CurrentTag
        {
            get
            {
                return _currentTag;
            }
            set
            {
                if (_currentTag != value)
                {
                    _currentTag = value;
                    RaisePropertyChanged(() => CurrentTag);
                }
            }
        }

        private plcdb_lib.Models.Model.ControllersRow _currentController;
        public plcdb_lib.Models.Model.ControllersRow CurrentController
        {
            get
            {
                if (_currentController == null && CurrentTag != null)
                {
                    _currentController = ActiveModel.Controllers.FindByPK(CurrentTag.Controller);
                }
                return _currentController;
            }
            set
            {
                if (_currentController != value)
                {
                    _currentController = value;
                    RaisePropertyChanged(() => CurrentController);
                    SetAvailableTags();
                }
            }
        }

        
        #endregion

        #region Commands

        public ICommand SaveCommand { get { return new DelegateCommand(OnSave); } }
        public ICommand CancelCommand { get { return new DelegateCommand(OnCancel); } }
        public ICommand RefreshTagsCommand { get { return new DelegateCommand(OnRefreshTags); } }

        
        #endregion

        #region Ctor
        public TagBrowserViewModel()
        {
            
        }
       
        #endregion

        #region Command Handlers

        private void OnSave()
        {
            CurrentTag.AcceptChanges();
        }

        private void OnCancel()
        {
            if (CurrentTag != null)
            {
                CurrentTag.RejectChanges();
            }
        }

        private void OnRefreshTags()
        {
            Model.TagsDataTable AllTags = CurrentController.Controller.BrowseTags();

            //add new tags
            foreach (Model.TagsRow row in AllTags)
            {
                if (ActiveModel.Tags.Where(p => p.Controller == CurrentController.PK && p.Address == row.Address).Count() == 0)
                {
                    Model.TagsRow NewRow = ActiveModel.Tags.NewTagsRow();
                    NewRow.Controller = CurrentController.PK;
                    foreach (DataColumn col in ActiveModel.Tags.Columns)
                    {
                        if (col.ColumnName != "PK")
                        {
                            NewRow[col.ColumnName] = row[col.ColumnName];
                        }
                    }
                    ActiveModel.Tags.AddTagsRow(NewRow);
                }
            }

            //delete old tags
            foreach (Model.TagsRow row in ActiveModel.Tags.Where(p => p.Controller == CurrentController.PK))
            {
                if (AllTags.Where(p => p.Address == row.Address).Count() == 0)
                {
                    row.Delete();
                }
            }

            ActiveModel.Tags.AcceptChanges();
            SetAvailableTags();
        }
        

        private void SetAvailableTags()
        {
            Model.TagsDataTable tbl = new Model.TagsDataTable();
            foreach (Model.TagsRow row in CurrentController.GetTagsRows())
            {
                tbl.ImportRow(row);
            }
            AvailableTags = tbl.AsEnumerable();
        }
        #endregion


    }
}