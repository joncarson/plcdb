using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using plcdb_lib;
using plcdb_lib.Models.Controllers;
using plcdb_lib.WCF;

namespace plcdb_lib.Models
{
    public partial class Model 
    {
        const string FILE_EXTENSION = ".plcdb";
        

        public Model Open(string path)
        {
            if (!path.EndsWith(FILE_EXTENSION))
            {
                path += FILE_EXTENSION;
            }

            if (!File.Exists(path))
            {
                return new Model();
            }
            FileStream Str = new FileStream(path, FileMode.Open);
            try
            {
                this.Clear();
                this.ReadXml(Str, System.Data.XmlReadMode.IgnoreSchema);
            }
            catch (Exception e)
            {
            }
            finally
            {
                Str.Close();
            }
            this.AcceptChanges();
            return this;
        }

        public void Save(string path)
        {
            this.AcceptChanges();
            if (!path.EndsWith(FILE_EXTENSION))
            {
                path += FILE_EXTENSION;
            }
            
            FileStream Str = new FileStream(path, FileMode.OpenOrCreate);
            try
            {
                this.WriteXml(Str, System.Data.XmlWriteMode.IgnoreSchema);
            }
            catch (Exception e)
            {
            }
            finally
            {
                Str.Close();
            }
        }

        public static List<Type> GetAllControllerTypes()
        {
            //find some dlls at runtime 
            string[] dlls = Directory.GetFiles(Environment.CurrentDirectory, "*.dll");

            List<Type> types = new List<Type>();
            //loop through the found dlls and load them 
            foreach (string dll in dlls)
            {
                System.Reflection.Assembly plugin = System.Reflection.Assembly.LoadFile(dll);
                foreach (Type t in plugin.GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(ControllerBase))))
                {
                    types.Add(t);
                }
            }

            return types;
        }



        public partial class ControllersRow
        {
            private ControllerBase _controller;
            public ControllerBase Controller
            {
                get
                {
                    if (_controller == null)
                    {
                        _controller = Activator.CreateInstance(this.Type, this) as ControllerBase;
                    }
                    return _controller;
                }
            }
        }

    }
}
