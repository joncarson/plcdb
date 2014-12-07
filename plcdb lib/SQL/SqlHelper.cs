using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plcdb_lib.SQL
{
    public static class SqlStringExtensions
    {
        
        public static String ToSqlString(this Object obj)
        {
            if (obj is String)
                return "'" + obj.ToString() + "'";

            else if (obj is Boolean)
                return String.Compare(obj.ToString(), "true", true) == 0 ? "1" : "0";

            else
                return obj.ToString();
        }

    }
}
