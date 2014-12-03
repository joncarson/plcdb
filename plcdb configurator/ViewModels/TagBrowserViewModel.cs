using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using plcdb.Helpers;
using plcdb_lib.Models;
using System.Data.SqlClient;
using System.Data;

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
                    var vm = App.Current.Resources["MainWindowViewModel"] as MainWindowViewModel;
                    _availableControllers = vm.ActiveModel.Controllers.Rows;
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
        private DataRowCollection _availableTags;
        public DataRowCollection AvailableTags
        {
            get
            {
                if (_availableTags == null)
                {
                    //var vm = App.Current.Resources["MainWindowViewModel"] as MainWindowViewModel;
                    _availableTags = ActiveModel.Tags.Rows;
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
                    UpdateAvailableTags();
                }
            }
        }

        
        #endregion

        #region Commands

        public ICommand SaveCommand { get { return new DelegateCommand(OnSave); } }
        public ICommand CancelCommand { get { return new DelegateCommand(OnCancel); } }

        
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

        private void UpdateAvailableTags()
        {
            MainWindowViewModel vm = App.Current.Resources["MainWindowViewModel"] as MainWindowViewModel;
            foreach (Model.TagsRow row in vm.ActiveModel.Tags.Where(p => p.Controller == CurrentController.PK))
            {
                row.Delete();
                //vm.ActiveModel.Tags.RemoveTagsRow(row);
            }
            foreach (Model.TagsRow row in CurrentController.Controller.BrowseTags())
            {
                row.Controller = CurrentController.PK;
                vm.ActiveModel.Tags.Rows.Add(row.ItemArray);

            }
            vm.ActiveModel.Tags.AcceptChanges();
            SetAvailableTags();
        }

        private void SetAvailableTags()
        {
            Model.TagsDataTable tbl = new Model.TagsDataTable();
            foreach (Model.TagsRow row in CurrentController.GetTagsRows())
            {
                tbl.ImportRow(row);
            }
            AvailableTags = tbl.Rows;
        }
        #endregion


    }
}