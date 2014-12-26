using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace plcdb_service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            LoadDynamicDLLs();
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new plcdb() 
            };
            ServiceBase.Run(ServicesToRun);
        }

        //gets driver plugins at startup (OPC, Siemens, etc)
        private static void LoadDynamicDLLs()
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
