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
using System.Windows.Shapes;
using plcdb_lib.Models;
using plcdb.ViewModels;

namespace plcdb.Views
{
    /// <summary>
    /// Interaction logic for NewControllerPopup.xaml
    /// </summary>
    public partial class QueryConfigPopup : Window
    {
        public QueryConfigPopup()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnTagBrowse_Click(object sender, RoutedEventArgs e)
        {
            Model.TagsRow Tag = null;
            if (e.Source == btnTriggerTag)
            {
                Tag = ((sender as Button).DataContext as QueryPopupViewModel).TriggerTag;
            }

            TagBrowserPopup popup = new TagBrowserPopup();
            
            popup.DataContext = new TagBrowserViewModel()
            {
                CurrentTag = Tag
            };
            popup.ShowDialog();

            if (popup.DialogResult.HasValue && popup.DialogResult.Value)
            {
                QueryPopupViewModel vm = this.DataContext as QueryPopupViewModel;
                vm.TriggerTag = (popup.DataContext as TagBrowserViewModel).CurrentTag;
            }
        }
    }
}
