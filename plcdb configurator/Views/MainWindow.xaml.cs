using Microsoft.Win32;
using plcdb.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Linq;
using Fluent;

namespace plcdb.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            LoadPreviousModelFile();
        }

        #region Open/save/close/exit commands
        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Open();//Implementation of open file
        }
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save();//Implementation of saveAs
        }
        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAs();//Implementation of saveAs
        }
        private void ExitCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Exit();//Implementation of exit
        }

        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CloseModel();//Implementation of exit
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
            MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
            if (vm.ActiveModel != null && File.Exists(vm.ActiveModelPath))
            {
                vm.OnSaveModel();
            }
            else
            {
                SaveAs();
            }
        }

        private void SaveAs()
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

        private void CloseModel()
        {
            MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
            vm.ActiveModel.Clear();
            vm.ActiveModelPath = "";
        }

        private void Exit()
        {
            this.Close();
        }

        private void LoadPreviousModelFile()
        {
            try
            {
                if (File.Exists(Properties.Settings.Default.LastOpenedFile))
                {
                    (this.DataContext as MainWindowViewModel).ActiveModel.Open(Properties.Settings.Default.LastOpenedFile);
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        private void TabControl_SelectionChanged_1(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            MainRibbon.DatabaseTab.Visibility   = MainTabControl.SelectedItem == DatabaseTab ? Visibility.Visible : Visibility.Collapsed;
            MainRibbon.ControllerTab.Visibility = MainTabControl.SelectedItem == ControllerTab ? Visibility.Visible : Visibility.Collapsed;
            MainRibbon.QueryTab.Visibility      = MainTabControl.SelectedItem == QueryTab ? Visibility.Visible : Visibility.Collapsed;
            MainRibbon.ServiceLogTab.Visibility = MainTabControl.SelectedItem == ServiceTab ? Visibility.Visible : Visibility.Collapsed;

            //MainRibbon.DatabaseTab.IsSelected = MainTabControl.SelectedItem == DatabaseTab ? true : false;
            //MainRibbon.ControllerTab.IsSelected = MainTabControl.SelectedItem == ControllerTab ? true : false;
            //MainRibbon.QueryTab.IsSelected = MainTabControl.SelectedItem == QueryTab ? true : false;
        }

        
    }
}
