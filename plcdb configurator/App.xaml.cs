using plcdb.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using plcdb_lib.HelperFunctions;
using System.Threading;
using plcdb.Helpers;

namespace plcdb
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoadDynamicDLLs();

            //check if in admin mode
            if (!UacHelper.IsProcessElevated && UacHelper.IsUacEnabled)
            {
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                proc.Verb = "runas";
                try
                {
                    Process.Start(proc);
                }
                catch
                {
                    // The user refused to allow privileges elevation.
                    return;
                }
                Application.Current.Shutdown();  // Quit itself
            }
            //check if opening a config file
            //if (e != null && e.Args != null && e.Args.Length > 0)
            //{
            //    if (File.Exists(e.Args[0]))
            //    {
            //        plcdb.Properties.Settings.Default.LastOpenedFile = e.Args[0];
            //        plcdb.Properties.Settings.Default.Save();
            //    }
            //}

            //Console.WriteLine(LicenseHelper.Unique_HW_ID());
        }


        //gets driver plugins at startup (OPC, Siemens, etc)
        private void LoadDynamicDLLs()
        {
            //find some dlls at runtime 
            string[] dlls = Directory.GetFiles(Environment.CurrentDirectory, "*plcdb*.dll");

            List<Type> types = new List<Type>();
            //loop through the found dlls and load them 
            foreach (string dll in dlls)
            {
                try
                {
                    System.Reflection.Assembly plugin = System.Reflection.Assembly.LoadFile(dll);
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
