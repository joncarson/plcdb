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
using NLog;

namespace plcdb_lib_opc
{
    public class OPC : ControllerBase
    {
        private DAServer Server = null;
        private Group TagGroup = null;
        private Model.ControllersRow ControllerInfo;
        List<OpcTag> ActiveTags = new List<OpcTag>();
        private Logger Log = LogManager.GetCurrentClassLogger();
        private static int ItemClientId = 1;
        private static int GroupClientId = 1;

        public static string Name
        {
            get { return "OPC DA"; }
        }

        public override object Read(plcdb_lib.Models.Model.TagsRow t)
        {
            try
            {
                if (!ActiveTags.Select(p => p.TagRow).Contains(t))
                {
                    AddTagToGroup(t);
                }
                return ActiveTags.First(p => p.TagRow.PK == t.PK).Value;
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading tag '" + t + "' from OPC server: " + ControllerInfo.Address + "\\" + ControllerInfo.opc_server, ex);
            }
        }

        public override bool Write(plcdb_lib.Models.Model.TagsRow t, object val)
        {
            try
            {
                if (!ActiveTags.Select(p => p.TagRow).Contains(t))
                {
                    AddTagToGroup(t);
                }
                OpcTag ToWrite = ActiveTags.First(p => p.TagRow.PK == t.PK);
                TagGroup.SyncWriteItems(new int[] { ToWrite.ServerId }, new object[] { val });
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error writing tag '" + t + "' from OPC server: " + ControllerInfo.Address + "\\" + ControllerInfo.opc_server, ex);
            }
        }

        public override plcdb_lib.Models.Model.TagsDataTable BrowseTags()
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception("Error browsing tags from OPC server: " + ControllerInfo.Address + "\\" + ControllerInfo.opc_server, ex);
            }
        }

        public OPC(Model.ControllersRow ControllerInfo)
        {
            try
            {
                this.ControllerInfo = ControllerInfo;
                String OpcName = "plcdb-" + ControllerInfo.PK;
                Server = new DAServer(ControllerInfo.opc_server, ControllerInfo.Address);

                TagGroup = Server.AddGroup(Interlocked.Increment(ref GroupClientId), OpcName, true, 100, (float)0.0);
                Model.TagsRow[] TagRows = ControllerInfo.GetTagsRows();
            }
            catch (Exception ex)
            {
                Log.Error("Error creating OPC connection: " + ex.Message);
            }
        }

        void TagGroup_ReadComplete(object sender, DataChangeEventArgs e)
        {
            foreach (ItemValue value in e.Values)
            {
                var Tag = ActiveTags.FirstOrDefault(p => p.ClientId == value.ClientId);
                if (Tag != null)
                    Tag.ItemValue = value;
            }
        }

        private void AddTagToGroup(Model.TagsRow tag)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception("Error adding tag '" + tag + "' to group on OPC server: " + ControllerInfo.Address + "\\" + ControllerInfo.opc_server, ex);
            }

        }
    }
}