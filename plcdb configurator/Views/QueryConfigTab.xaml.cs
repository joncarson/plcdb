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
    /// Interaction logic for ConfigTab.xaml
    /// </summary>
    public partial class QueryConfigTab : UserControl
    {
        public QueryConfigTab()
        {
            InitializeComponent();
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

        private void btnEditQuery_Click(object sender, RoutedEventArgs e)
        {
            QueryConfigPopup popup = new QueryConfigPopup();
            popup.DataContext = new QueryPopupViewModel()
            {
                CurrentQuery = (Model.QueriesRow)((System.Data.DataRowView)QueriesGrid.SelectedValue).Row
            };
            popup.ShowDialog();
        }
    }
}
