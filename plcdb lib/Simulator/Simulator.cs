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
        
        public static string Name
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
                case "DateTimes\\Now":
                    return DateTime.Now;
                    break;
            }
            throw new AddressNotFoundException();
        }

        public override bool Write(Models.Model.TagsRow t, object val)
        {
            throw new NotImplementedException();
        }

        private Model.TagsDataTable Tags;
        public override Model.TagsDataTable BrowseTags()
        {
            if (Tags == null)
            {
                Tags = new Model.TagsDataTable();
                Model.TagsRow Sine = Tags.NewTagsRow();
                Sine.Address = "Doubles\\Sine";
                Sine.Name = Sine.Address;
                Sine.DataType = typeof(Double);
                Sine.Controller = ControllerInfo.PK;

                Model.TagsRow Rand = Tags.NewTagsRow();
                Rand.Address = "Doubles\\Rand";
                Rand.Name = Rand.Address;
                Rand.DataType = typeof(Double);
                Rand.Controller = ControllerInfo.PK;

                Model.TagsRow AlwaysOn = Tags.NewTagsRow();
                AlwaysOn.Address = "Bools\\AlwaysOn";
                AlwaysOn.Name = AlwaysOn.Address;
                AlwaysOn.DataType = typeof(Boolean);
                AlwaysOn.Controller = ControllerInfo.PK;

                Model.TagsRow AlwaysOff = Tags.NewTagsRow();
                AlwaysOff.Address = "Bools\\AlwaysOff";
                AlwaysOff.Name = AlwaysOff.Address;
                AlwaysOff.DataType = typeof(Boolean);
                AlwaysOff.Controller = ControllerInfo.PK;


                Model.TagsRow RandBool = Tags.NewTagsRow();
                RandBool.Address = "Bools\\RandBool";
                RandBool.Name = RandBool.Address;
                RandBool.DataType = typeof(Boolean);
                RandBool.Controller = ControllerInfo.PK;

                Model.TagsRow Now = Tags.NewTagsRow();
                Now.Address = "DateTimes\\Now";
                Now.Name = Now.Address;
                Now.DataType = typeof(DateTime);
                Now.Controller = ControllerInfo.PK;

                Tags.AddTagsRow(Sine);
                Tags.AddTagsRow(Rand);
                Tags.AddTagsRow(AlwaysOn);
                Tags.AddTagsRow(AlwaysOff);
                Tags.AddTagsRow(RandBool);
                Tags.AddTagsRow(Now);
            }
            return Tags;
        }

        public  Simulator(Model.ControllersRow row)
        {
            ControllerInfo = row;
        }


        private Model.ControllersRow ControllerInfo { get; set; }

        public override bool ValidateTag(string address)
        {
            if (Tags == null)
                return false;

            return Tags.Select(p => p.Address).Contains(address);
        }
    }
}
