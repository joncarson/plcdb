using plcdb_lib.Models;
using ProcessControlStandards.OPC.DataAccessClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plcdb_lib_opc
{
    class OpcTag
    {
        public Model.TagsRow TagRow { get; set; }
        public int ServerId  //server-side reference (assigned by OPC server)
        {
            get
            {
                return ItemResult.ServerId;
            }
        }
        public int ClientId //client-side reference (assigned by us)
        {
            get
            {
                return Item.ClientId;
            }
        }
        public String ItemId //what we think of as the actual OPC address
        {
            get
            {
                return Item.ItemId;
            }
        }
        public object Value
        {
            get
            {
                return ItemValue.Value;
            }
        }
        public Item Item {get;set;} //ProcessControlStandards library object
        public ItemResult ItemResult { get; set; } //ProcessControlStandards library object
        public ItemValue ItemValue { get; set; }
    }
}
