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
    /// Interaction logic for DatabaseConfigTab.xaml
    /// </summary>
    public partial class ControllerConfigTab : UserControl
    {
        public ControllerConfigTab()
        {
            InitializeComponent();
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

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ControllerConfigPopup popup = new ControllerConfigPopup();
            popup.DataContext = new ControllerPopupViewModel()
            {
                CurrentController = (Model.ControllersRow)((System.Data.DataRowView)ControllerGrid.SelectedValue).Row
            };
            popup.ShowDialog();
        }
    }
}
