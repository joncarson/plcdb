using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using plcdb_lib;
using plcdb_lib.Models.Controllers;
using plcdb_lib.WCF;
using System.Data;
using NLog;

namespace plcdb_lib.Models
{
    public partial class Model 
    {
        private Logger Log = LogManager.GetCurrentClassLogger();
        partial class ControllersDataTable
        {
        }
    
        const string FILE_EXTENSION = ".plcdb";
        public event EventHandler DatasetChanged;
 
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

            
            foreach (DataTable tbl in this.Tables)
            {
                //tbl.RowChanged -= RaiseDatasetChangedEvent;
                tbl.ColumnChanging-= RaiseDatasetChangedEvent;
                //tbl.RowChanged += RaiseDatasetChangedEvent;
                tbl.ColumnChanging += RaiseDatasetChangedEvent;
            }
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

        public void RaiseDatasetChangedEvent(object sender, DataColumnChangeEventArgs args)
        {
            if (args.Column.ColumnName != "Status")
            {
                if (args.ProposedValue == null ||  args.ProposedValue.ToString() != args.Row[args.Column].ToString())
                {
                    if (DatasetChanged != null)
                    {
                        DatasetChanged(this, null);
                    }
                }
            }
        }

        public static List<Type> GetAllControllerTypes()
        {
            //find some dlls at runtime 
            string[] dlls = Directory.GetFiles(Environment.CurrentDirectory, "*plcdb*.dll");

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
                        try
                        {
                            _controller = Activator.CreateInstance(this.Type, this) as ControllerBase;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error creating controller", ex);
                        }
                    }
                    return _controller;
                }
            }
        }

        public partial class TagsRow
        {
            public override string ToString()
            {
                try
                {
                    if (this == null)
                    {
                        return "";
                    }
                    String TagName = this.Name == null || this.Name == String.Empty ? this.Address : this.Name;

                    return "[" + this.ControllersRow.Name + "]" + TagName;
                }
                catch (Exception e)
                {
                    return base.ToString();
                }
            }
        }
    }
}
