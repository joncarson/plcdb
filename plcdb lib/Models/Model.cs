using System;
using System.IO;
using System.Xml.Serialization;
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
    }
}
