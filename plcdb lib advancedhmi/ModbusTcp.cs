using plcdb_lib.Models.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedHMIDrivers;
using plcdb_lib.Models;

namespace plcdb_lib_advancedhmi
{
    public class ModbusTcp : ControllerBase
    {
        ModbusTCPCom Comm = new ModbusTCPCom();

        public static string Name
        {
            get { return "Modbus TCP"; }
        }

        public ModbusTcp(Model.ControllersRow ControllerInfo)
        {
            Comm.IPAddress = ControllerInfo.Address;
            Comm.TcpipPort = 502;
        }

        public override object Read(Model.TagsRow t)
        {
            t.CurrentValue = Comm.Read(t.Address);
            return t.CurrentValue;
        }

        public override bool Write(Model.TagsRow t, object val)
        {
                Comm.Write(t.Address, val.ToString());
                return true;
        }

        public override Model.TagsDataTable BrowseTags()
        {
            return new Model.TagsDataTable();
        }

        public override List<string> GetAvailableSubaddresses()
        {
            throw new NotImplementedException();
        }
    }
}
