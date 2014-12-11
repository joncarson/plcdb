using plcdb_lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Snap7;

namespace plcdb_lib_siemens_s7
{
    public class SiemensTag
    {
        public Model.TagsRow TagRow { get; set; }

        public int Start { get; set; }

        public AddressLength Length { get; set; }

        public AddressSpaceTypes AddressSpace
        {
            get
            {
                if (TagRow.Address.Substring(0, 2).ToUpper() == "DB")
                    return AddressSpaceTypes.DB;
                else
                {
                    return (AddressSpaceTypes)Enum.Parse(typeof(AddressSpaceTypes), TagRow.Address.Substring(0,1).ToUpper());
                }
            }
        }
        public int DB
        {
            get
            {
                try
                {
                    return Int32.Parse(Regex.Match(TagRow.Address.ToUpper(), "^DB([0-9]+)").Value);
                }
                catch (Exception e)
                {
                    return -1;
                }
            }
        }

        public enum AddressSpaceTypes
        {
            DB = S7Client.S7AreaDB,
            M = S7Client.S7AreaMK,
            C = S7Client.S7AreaCT,
            T = S7Client.S7AreaTM,
            In = S7Client.S7AreaPE,
            Out = S7Client.S7AreaPA
        }

        public enum AddressLength
        {
            Bit = S7Client.S7WLBit,
            Byte = S7Client.S7WLByte,
            Counter = S7Client.S7WLCounter,
            DWord = S7Client.S7WLDWord,
            Real = S7Client.S7WLReal,
            Timer = S7Client.S7WLTimer,
            Word = S7Client.S7WLWord
        }
    }
}
