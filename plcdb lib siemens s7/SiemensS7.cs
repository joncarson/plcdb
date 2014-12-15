using plcdb_lib.Models.Controllers;
using Snap7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using plcdb_lib.Models;
using System.Timers;

namespace plcdb_lib_siemens_s7
{
    public class SiemensS7 : ControllerBase
    {
        S7Client Client;
        Model.ControllersRow ControllerInfo;
        private List<SiemensTag> ActiveTags;
        private byte[] Buffer = new byte[1024];
        Timer RefreshDataTimer = new Timer();
        public static string Name
        {
            get { return "Siemens S7"; }
        }

        public SiemensS7(Model.ControllersRow ControllerInfo)
        {
            this.ControllerInfo = ControllerInfo;
            ActiveTags = new List<SiemensTag>();

            Client = new S7Client();
            Client.ConnectTo(ControllerInfo.Address, 0, 2);

            RefreshDataTimer.Interval = 1000;
            RefreshDataTimer.Elapsed += RefreshDataTimer_Elapsed;
        }

        void RefreshDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Client.Connected())
            {
                Client.ConnectTo(ControllerInfo.Address, 0, 2);
            }

            foreach (SiemensTag Tag in ActiveTags)
            {
                int result = Client.ReadArea((int)Tag.AddressSpace, Tag.DB, Tag.Start, 1, (int)Tag.Length, Buffer);
                Tag.TagRow.CurrentValue = Buffer[0];
            }
        }


        public override object Read(plcdb_lib.Models.Model.TagsRow t)
        {
            SiemensTag Tag = ActiveTags.FirstOrDefault(p => p.TagRow.PK == t.PK);
            if (Tag == null)
            {
                ActiveTags.Add(new SiemensTag()
                {
                    TagRow = t
                });
            }
            return Tag.TagRow.CurrentValue;
        }

        public override bool Write(plcdb_lib.Models.Model.TagsRow t, object val)
        {
            throw new NotImplementedException();
        }

        public override Model.TagsDataTable BrowseTags()
        {
            return new Model.TagsDataTable();
        }

    }
}
