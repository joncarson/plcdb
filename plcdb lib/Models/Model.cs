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

            XmlSerializer Xml = new XmlSerializer(typeof(Model));
            FileStream Str = new FileStream(path, FileMode.Open);
            return (Model)Xml.Deserialize(Str);
        }

        public void Save(string path)
        {
            if (!path.EndsWith(FILE_EXTENSION))
            {
                path += FILE_EXTENSION;
            }

            XmlSerializer Xml = new XmlSerializer(typeof(Model));
            FileStream Str = new FileStream(path, FileMode.OpenOrCreate);
            Xml.Serialize(Str, this);
        }
    }
}
