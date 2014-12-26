using plcdb_lib.Models.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedHMIDrivers;
using plcdb_lib.Models;
using System.Text.RegularExpressions;
using System.ComponentModel;

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
            
            if (ControllerInfo.Address == "localhost")
                Comm.IPAddress = "127.0.0.1";
            else
                Comm.IPAddress = ControllerInfo.Address;
            Comm.TcpipPort = (ushort)ControllerInfo.modbus_port;
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


        public override bool ValidateTag(string address)
        {
            return Regex.IsMatch(address, @"[0|1|3|4][0-9][0-9][0-9][0-9]");
        }
    }
}
