using plcdb.ViewModels;
using plcdb_lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace plcdb.Views
{
    /// <summary>
    /// Interaction logic for MainRibbon.xaml
    /// </summary>
    public partial class MainRibbon : UserControl
    {
        MainWindowViewModel vm
        {
            get
            {
                return this.DataContext as MainWindowViewModel;
            }
        }

        public MainRibbon()
        {
            InitializeComponent();
        }


        private void btnAddNewDatabase_Click(object sender, RoutedEventArgs e)
        {
            DatabaseConfigPopup popup = new DatabaseConfigPopup();

            Model.DatabasesRow NewRow = vm.ActiveModel.Databases.NewDatabasesRow();
            NewRow.Name = "New Database";
            vm.ActiveModel.Databases.AddDatabasesRow(NewRow);
            popup.DataContext = new DatabasePopupViewModel()
            {
                CurrentDatabase = NewRow,
            };
            popup.ShowDialog();
        
        }

        private void btnDeleteDatabase_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult Confirm = MessageBox.Show("Are you sure you want to delete this database?", "Warning!", MessageBoxButton.OKCancel);
            if (Confirm == MessageBoxResult.OK)
            {
                vm.DeleteRow(vm.SelectedDatabase);
            }
        }

        private void btnEditDatabase_Click(object sender, RoutedEventArgs e)
        {
            DatabaseConfigPopup popup = new DatabaseConfigPopup();
            popup.DataContext = new DatabasePopupViewModel()
            {
                CurrentDatabase = vm.SelectedDatabase
            };
            popup.ShowDialog();
        }

        private void btnAddNewController_Click(object sender, RoutedEventArgs e)
        {
            ControllerConfigPopup popup = new ControllerConfigPopup();
            var vm = this.DataContext as MainWindowViewModel;

            Model.ControllersRow NewRow = vm.ActiveModel.Controllers.NewControllersRow();
            NewRow.Name = "New Controller";
            NewRow.Address = "";
            NewRow.Type = null;
            vm.ActiveModel.Controllers.AddControllersRow(NewRow);
            popup.DataContext = new ControllerPopupViewModel()
            {
                CurrentController = NewRow,
            };
            popup.ShowDialog();
        }

        private void btnDeleteController_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult Confirm = MessageBox.Show("Are you sure you want to delete this PLC?", "Warning!", MessageBoxButton.OKCancel);
            if (Confirm == MessageBoxResult.OK)
            {
                vm.DeleteRow(vm.SelectedController);
            }
        }

        private void btnEditController_Click(object sender, RoutedEventArgs e)
        {
            ControllerConfigPopup popup = new ControllerConfigPopup();
            popup.DataContext = new ControllerPopupViewModel()
            {
                CurrentController = vm.SelectedController
            };
            popup.ShowDialog();
        }

        private void btnAddQuery_Click(object sender, RoutedEventArgs e)
        {
            QueryConfigPopup popup = new QueryConfigPopup();
            var vm = this.DataContext as MainWindowViewModel;
            Model.QueriesRow NewRow = vm.ActiveModel.Queries.NewQueriesRow();
            NewRow.Name = "New Query";
            NewRow.MappingType = "";
            NewRow.MaxRows = 1;
            NewRow.QueryText = "";
            NewRow.QueryType = "";
            NewRow.RefreshRate = 5000;
            vm.ActiveModel.Queries.AddQueriesRow(NewRow);


            popup.DataContext = new QueryPopupViewModel()
            {
                CurrentQuery = NewRow,
            };
            popup.ShowDialog();
        }
        private void btnDeleteQuery_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult Confirm = MessageBox.Show("Are you sure you want to delete this query?", "Warning!", MessageBoxButton.OKCancel);
            if (Confirm == MessageBoxResult.OK)
            {
                vm.DeleteRow(vm.SelectedQuery);
            }
        }

        private void btnEditQuery_Click(object sender, RoutedEventArgs e)
        {
            QueryConfigPopup popup = new QueryConfigPopup();
            popup.DataContext = new QueryPopupViewModel()
            {
                CurrentQuery = vm.SelectedQuery
            };
            popup.ShowDialog();
        }
    }
}
