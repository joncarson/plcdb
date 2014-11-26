using plcdb_lib.Models.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using plcdb_lib.Models;

namespace plcdb_lib
{
    class Simulator : ControllerBase
    {
        public override string CONTROLLER_TYPE
        {
            get
            {
                return "Simulator";
            }
        }

        public override object read(Models.Model.TagsRow t)
        {
            switch (t.Address)
            {
                case "Doubles\\Sine":
                    return Math.Sin(DateTime.Now.Millisecond);
                    break;
                case "Doubles\\Rand":
                    return (new Random()).NextDouble();
                    break;
                case "Bool\\AlwaysOn":
                    return true;
                    break;
                case "Bool\\AlwaysOff":
                    return false;
                    break;
                case "Bool\\RandBool":
                    return ((new Random()).NextDouble()) >= 0.5;
                    break;
            }
            throw new AddressNotFoundException();
        }

        public override bool write(Models.Model.TagsRow t, object val)
        {
            throw new NotImplementedException();
        }

        public override Model.TagsDataTable browse_tags()
        {
            Model.TagsDataTable table = new Model.TagsDataTable();
            Model.TagsRow Sine = table.NewTagsRow();
            Sine.Name = "Sine";
            Sine.Address = "Doubles\\Sine";
            Sine.DataType = typeof(Double);
            
            Model.TagsRow Rand = table.NewTagsRow();
            Rand.Name = "Rand";
            Rand.Address = "Doubles\\Rand";
            Rand.DataType = typeof(Double);
            
            Model.TagsRow AlwaysOn = table.NewTagsRow();
            AlwaysOn.Name = "AlwaysOn";
            AlwaysOn.Address = "Bools\\AlwaysOn";
            AlwaysOn.DataType = typeof(Boolean);
            
            Model.TagsRow AlwaysOff = table.NewTagsRow();
            AlwaysOff.Name = "AlwaysOff";
            AlwaysOff.Address = "Bools\\AlwaysOff";
            AlwaysOff.DataType = typeof(Boolean);

            Model.TagsRow RandBool = table.NewTagsRow();
            RandBool.Name = "RandBool";
            RandBool.Address = "Bools\\RandBool";
            RandBool.DataType = typeof(Boolean);

            table.AddTagsRow(Sine);
            table.AddTagsRow(Rand);
            table.AddTagsRow(AlwaysOn);
            table.AddTagsRow(AlwaysOff);
            table.AddTagsRow(RandBool);
            
            return table;
        }
    }
}
