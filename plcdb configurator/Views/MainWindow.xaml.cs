using Microsoft.Win32;
using plcdb.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace plcdb.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Open();//Implementation of open file
        }
        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save();//Implementation of saveAs
        }
        private void ExitCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Exit();//Implementation of exit
        }

        private void Open()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            bool result = (bool)dlg.ShowDialog();
            if (result)
            {
                MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
                vm.ActiveModelPath = dlg.FileName;
                vm.OnLoadModel();
            }
        }

        private void Save()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            bool result = (bool)dlg.ShowDialog();
            if (result)
            {
                MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
                vm.ActiveModelPath = dlg.FileName;
                vm.OnSaveModel();
            }
        }

        private void Exit()
        {
            this.Close();
        }

    }
}
