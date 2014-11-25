using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plcdb_lib.Models.Controllers
{
    public abstract class ControllerBase
    {
        public abstract object read(Model.TagsRow t);
        public  Model.TagsDataTable read_all(Model.TagsDataTable set)
        {
            foreach (Model.TagsRow t in set.Rows)
            {
                t.CurrentValue = Convert.ChangeType(this.read(t), t.DataType);
            }
            return set;
        }

        public abstract bool write(Model.TagsRow t, object val);
        public bool write_all(Model.TagsDataTable set)
        {
            var success = true;
            foreach (Model.TagsRow t in set.Rows)
            {
                if (!this.write(t, t.CurrentValue))
                {
                    success = false;
                }
            }
            return success;
        }
    }
}
