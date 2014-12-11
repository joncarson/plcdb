using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

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
                System.Reflection.Assembly plugin = System.Reflection.Assembly.LoadFile(dll);
            }
        }
    }
}
