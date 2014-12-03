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
            LoadDynamicDLLs();
        }

        private void LoadDynamicDLLs()
        {
            //find some dlls at runtime 
            string[] dlls = Directory.GetFiles(Environment.CurrentDirectory, "*.dll");

            List<Type> types = new List<Type>();
            //loop through the found dlls and load them 
            foreach (string dll in dlls)
            {
                System.Reflection.Assembly plugin = System.Reflection.Assembly.LoadFile(dll);
            }
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

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
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

    }
}
