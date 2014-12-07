using plcdb_lib.Models.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using plcdb_lib.Models;
using NLog;


namespace plcdb_lib
{
    class Simulator : ControllerBase
    {
        
        public override string Name
        {
            get
            {
                return "Simulator";
                
            }
        }

        public override object Read(Models.Model.TagsRow t)
        {
            switch (t.Address)
            {
                case "Doubles\\Sine":
                    return Math.Sin(DateTime.Now.Millisecond);
                    break;
                case "Doubles\\Rand":
                    return (new Random()).NextDouble();
                    break;
                case "Bools\\AlwaysOn":
                    return true;
                    break;
                case "Bools\\AlwaysOff":
                    return false;
                    break;
                case "Bools\\RandBool":
                    return ((new Random()).NextDouble()) >= 0.5;
                    break;
            }
            throw new AddressNotFoundException();
        }

        public override bool Write(Models.Model.TagsRow t, object val)
        {
            throw new NotImplementedException();
        }

        private Model.TagsDataTable _browseTags;
        public override Model.TagsDataTable BrowseTags()
        {
            if (_browseTags == null)
            {
                _browseTags = new Model.TagsDataTable();
                Model.TagsRow Sine = _browseTags.NewTagsRow();
                Sine.Address = "Doubles\\Sine";
                Sine.Name = Sine.Address;
                Sine.DataType = typeof(Double);

                Model.TagsRow Rand = _browseTags.NewTagsRow();
                Rand.Address = "Doubles\\Rand";
                Rand.Name = Rand.Address;
                Rand.DataType = typeof(Double);

                Model.TagsRow AlwaysOn = _browseTags.NewTagsRow();
                AlwaysOn.Address = "Bools\\AlwaysOn";
                AlwaysOn.Name = AlwaysOn.Address;
                AlwaysOn.DataType = typeof(Boolean);

                Model.TagsRow AlwaysOff = _browseTags.NewTagsRow();
                AlwaysOff.Address = "Bools\\AlwaysOff";
                AlwaysOff.Name = AlwaysOff.Address;
                AlwaysOff.DataType = typeof(Boolean);

                Model.TagsRow RandBool = _browseTags.NewTagsRow();
                RandBool.Address = "Bools\\RandBool";
                RandBool.Name = RandBool.Address;
                RandBool.DataType = typeof(Boolean);

                _browseTags.AddTagsRow(Sine);
                _browseTags.AddTagsRow(Rand);
                _browseTags.AddTagsRow(AlwaysOn);
                _browseTags.AddTagsRow(AlwaysOff);
                _browseTags.AddTagsRow(RandBool);
            }
            return _browseTags;
        }

        public  Simulator(Model.ControllersRow row)
        {
        }

        public override List<string> GetAvailableSubaddresses()
        {
            return new List<string>();
        }
    }
}
