using plcdb.ViewModels;
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
using plcdb_lib.Models;

namespace plcdb.Views
{
    /// <summary>
    /// Interaction logic for DatabaseConfigTab.xaml
    /// </summary>
    public partial class DatabaseConfigTab : UserControl
    {
        public DatabaseConfigTab()
        {
            InitializeComponent();
        }

        private void btnAddNewDatabase_Click(object sender, RoutedEventArgs e)
        {
            DatabaseConfigPopup popup = new DatabaseConfigPopup();
            var vm = this.DataContext as MainWindowViewModel;
            Model.DatabasesRow NewRow = vm.ActiveModel.Databases.NewDatabasesRow();
            NewRow.Name = "New Database";
            vm.ActiveModel.Databases.AddDatabasesRow(NewRow);
            popup.DataContext = new DatabasePopupViewModel()
            {
                CurrentDatabase = NewRow,
            };
            popup.ShowDialog();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            DatabaseConfigPopup popup = new DatabaseConfigPopup();
            popup.DataContext = new DatabasePopupViewModel()
            {
                CurrentDatabase = (Model.DatabasesRow)((System.Data.DataRowView)DatabaseGrid.SelectedValue).Row
            };
            popup.ShowDialog();
        }
    }
}
