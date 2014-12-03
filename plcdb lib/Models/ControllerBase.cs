using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plcdb_lib.Models.Controllers
{
    public abstract class ControllerBase
    {
        public abstract String Name { get; }
        public abstract object Read(Model.TagsRow t);
        public  Model.TagsDataTable ReadAll(Model.TagsDataTable set)
        {
            foreach (Model.TagsRow t in set.Rows)
            {
                t.CurrentValue = Convert.ChangeType(this.Read(t), t.DataType);
            }
            return set;
        }

        public abstract bool Write(Model.TagsRow t, object val);
        public bool WriteAll(Model.TagsDataTable set)
        {
            var success = true;
            foreach (Model.TagsRow t in set.Rows)
            {
                if (!this.Write(t, t.CurrentValue))
                {
                    success = false;
                }
            }
            return success;
        }

        public abstract Model.TagsDataTable BrowseTags();

        protected ControllerBase(Model.ControllersRow args)
        {
        }

        protected ControllerBase()
        {
        }
    }

    public class AddressNotFoundException : Exception
    {
        public AddressNotFoundException() :  base("Read failed. Address not found in PLC.")
        {
            
        }

        public AddressNotFoundException(string message)
            : base(message)
        {
        }

        public AddressNotFoundException(string message,
          Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
