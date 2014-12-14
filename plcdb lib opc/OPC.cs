using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using plcdb_lib.Models;
using plcdb_lib.Models.Controllers;
using ProcessControlStandards.OPC;
using ProcessControlStandards.OPC.DataAccessClient;
using plcdb_lib;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using ProcessControlStandards.OPC.Core;

namespace plcdb_lib_opc
{
    public class OPC : ControllerBase
    {
        private DAServer Server = null;
        private Group TagGroup = null;
        private Model.ControllersRow ControllerInfo;
        List<OpcTag> ActiveTags = new List<OpcTag>();

        private static int ItemClientId = 1;
        private static int GroupClientId = 1;

        public static string Name
        {
            get { return "OPC DA"; }
        }

        public override object Read(plcdb_lib.Models.Model.TagsRow t)
        {
            if (!ActiveTags.Select(p => p.TagRow).Contains(t))
            {
                AddTagToGroup(t);
            }
            return ActiveTags.First(p => p.TagRow.PK == t.PK).Value;
        }

        public override bool Write(plcdb_lib.Models.Model.TagsRow t, object val)
        {
            if (!ActiveTags.Select(p => p.TagRow).Contains(t))
            {
                AddTagToGroup(t);
            }
            OpcTag ToWrite = ActiveTags.First(p => p.TagRow.PK == t.PK);
            TagGroup.SyncWriteItems(new int[] {ToWrite.ServerId}, new object[] {val});
            return true;
        }

        public override plcdb_lib.Models.Model.TagsDataTable BrowseTags()
        {
            Model.TagsDataTable Table = new Model.TagsDataTable();

            if (Server == null || TagGroup == null)
                return Table;

            ServerAddressSpaceBrowser Browser = Server.GetAddressSpaceBrowser();
            foreach (String ItemId in Browser.GetItemIds(BrowseType.Flat, string.Empty, VarEnum.VT_EMPTY, 0))
            {
                Model.TagsRow Row = Table.NewTagsRow();
                Row.Address = ItemId;
                Row.Controller = ControllerInfo.PK;
                Table.AddTagsRow(Row);
            }
            Table.AcceptChanges();
            return Table;
        }

        public OPC(Model.ControllersRow ControllerInfo)
        {
            this.ControllerInfo = ControllerInfo;
            String OpcName =  "plcdb-" + ControllerInfo.PK;
            Server = new DAServer(ControllerInfo.Address);
            
            TagGroup = Server.AddGroup(Interlocked.Increment(ref GroupClientId), OpcName, true, 100, (float)0.0);
            Model.TagsRow[] TagRows = ControllerInfo.GetTagsRows();
        }

        void TagGroup_ReadComplete(object sender, DataChangeEventArgs e)
        {
            foreach (ItemValue value in e.Values)
            {
                ActiveTags.First(p => p.ClientId == value.ClientId).ItemValue = value;
            }
        }

        private System.Runtime.InteropServices.VarEnum TypeToOpcType(Type type)
        {
            if (type == typeof(int))
                return System.Runtime.InteropServices.VarEnum.VT_INT;
            else if (type == typeof(float) || type == typeof(double))
                return System.Runtime.InteropServices.VarEnum.VT_DECIMAL;
            else if (type == typeof(string))
                return System.Runtime.InteropServices.VarEnum.VT_LPSTR;
            else if (type == typeof(bool))
                return System.Runtime.InteropServices.VarEnum.VT_BOOL;
            //else if (type == typeof(char))
            //    return System.Runtime.InteropServices.VarEnum.VT_BYTE;
            return System.Runtime.InteropServices.VarEnum.VT_UNKNOWN;
        }

        public override List<string> GetAvailableSubaddresses()
        {
            ServerBrowser browser = new ServerBrowser();
            return new List<string>();
        }

        private void AddTagToGroup(Model.TagsRow tag)
        {
            OpcTag NewTag = new OpcTag();
            NewTag.TagRow = tag;
            NewTag.Item = new Item()
            {
                //AccessPath = TagRow.Address,
                Active = true,
                ClientId = (int)tag.PK,
                ItemId = tag.Address,
                RequestedDataType = VarEnum.VT_EMPTY// TagRow.IsDataTypeNull() ? VarEnum.VT_UNKNOWN : TypeToOpcType(TagRow.DataType)
            };
            NewTag.ItemValue = new ItemValue();
            
            var TagResult = TagGroup.AddItems(new Item[] { NewTag.Item }).First();
            NewTag.ItemResult = TagResult;

            ActiveTags.Add(NewTag);
            //add event handler -- make sure we are not adding a duplicate copy
            TagGroup.ReadComplete -= TagGroup_ReadComplete;
            TagGroup.DataChange -= TagGroup_ReadComplete;
            TagGroup.ReadComplete += TagGroup_ReadComplete;
            TagGroup.DataChange += TagGroup_ReadComplete;
        }
    }
}