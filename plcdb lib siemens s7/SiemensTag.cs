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
        public static Regex DbRegex = new Regex(@"^DB[1-9]+[0-9]*[0-9]*[0-9]*\.DB((X[0-9]+[0-9]*[0-9]*[0-9]*\.[0-7])|W[0-9]+[0-9]*[0-9]*[0-9]*|D[0-9]+[0-9]*[0-9]*[0-9]*|B[0-9]+[0-9]*[0-9]*[0-9]*)$");
        public static Regex MRegex = new Regex(@"^M([0-9]+[0-9]*[0-9]*[0-9]*\.[0-7]|W[0-9]+[0-9]*[0-9]*[0-9]*|D[0-9]+[0-9]*[0-9]*[0-9]*|B[0-9]+[0-9]*[0-9]*[0-9]*)$");
        public static Regex IRegex = new Regex(@"^I([0-9]+[0-9]*[0-9]*[0-9]*\.[0-7]|W[0-9]+[0-9]*[0-9]*[0-9]*|D[0-9]+[0-9]*[0-9]*[0-9]*|B[0-9]+[0-9]*[0-9]*[0-9]*)$");
        public static Regex QRegex = new Regex(@"^Q([0-9]+[0-9]*[0-9]*[0-9]*\.[0-7]|W[0-9]+[0-9]*[0-9]*[0-9]*|D[0-9]+[0-9]*[0-9]*[0-9]*|B[0-9]+[0-9]*[0-9]*[0-9]*)$");
        public static Regex TRegex = new Regex(@"^T([0-9]+[0-9]*[0-9]*[0-9]*\.[0-7]|W[0-9]+[0-9]*[0-9]*[0-9]*|D[0-9]+[0-9]*[0-9]*[0-9]*|B[0-9]+[0-9]*[0-9]*[0-9]*)$");
        public static Regex CRegex = new Regex(@"^C([0-9]+[0-9]*[0-9]*[0-9]*\.[0-7]|W[0-9]+[0-9]*[0-9]*[0-9]*|D[0-9]+[0-9]*[0-9]*[0-9]*|B[0-9]+[0-9]*[0-9]*[0-9]*)$");

        private Model.TagsRow _tagRow;
        public Model.TagsRow TagRow
        {
            get
            {
                return _tagRow;
            }
            set
            {
                _tagRow = value;
                SetPropertiesFromTagRow(value);
            }
        }

        private void SetPropertiesFromTagRow(Model.TagsRow value)
        {
            SiemensTag Tag = CreateTag(value.Address);
            Start = Tag.Start;
            Length = Tag.Length;
            Bit = Tag.Bit;
        }

        public int Start { get; set; }

        public int Bit { get; set; }
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
                    return Int32.Parse(Regex.Match(TagRow.Address.ToUpper(), "^DB[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(2));
                }
                catch (Exception e)
                {
                    return 1;
                }
            }
        }

        public enum AddressSpaceTypes
        {
            DB = S7Client.S7AreaDB,
            M = S7Client.S7AreaMK,
            C = S7Client.S7AreaCT,
            T = S7Client.S7AreaTM,
            I = S7Client.S7AreaPE,
            Q = S7Client.S7AreaPA
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

        public static SiemensTag CreateTag(String address)
        {
            
            SiemensTag NewTag = new SiemensTag();
            NewTag.Bit = -1;
            address = address.ToUpper();
            if (DbRegex.IsMatch(address))
            {
                char AddressType = Regex.Match(address, "(DBB|DBX|DBW|DBD)").Value[2];
                if (AddressType == 'X')
                {
                    NewTag.Length = AddressLength.Bit;
                    NewTag.Start = int.Parse(Regex.Match(address, @"DBX[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(3));
                    NewTag.Bit = int.Parse(Regex.Match(address, @"\.[0-7]").Value.Substring(1));
                }
                else if (AddressType == 'B')
                {
                    NewTag.Length = AddressLength.Byte;
                    NewTag.Start = int.Parse(Regex.Match(address, @"DBB[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(3));
                }
                else if (AddressType == 'W')
                {
                    NewTag.Length = AddressLength.Word;
                    NewTag.Start = int.Parse(Regex.Match(address, @"DBW[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(3));
                }
                else if (AddressType == 'D')
                {
                    NewTag.Length = AddressLength.DWord;
                    NewTag.Start = int.Parse(Regex.Match(address, @"DBD[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(3));
                }
            }
            else if (MRegex.IsMatch(address))
            {
                Match AddressTypeMatch = Regex.Match(address, "(MB|MW|MD)");
                if (!AddressTypeMatch.Success)
                {
                        NewTag.Length = AddressLength.Bit;
                        NewTag.Start = int.Parse(Regex.Match(address, @"M[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(1));
                        NewTag.Bit = int.Parse(Regex.Match(address, @"\.[0-7]").Value.Substring(1));
                }
                else
                {
                    char AddressType = AddressTypeMatch.Value[1];
                    if (AddressType == 'B')
                    {
                        NewTag.Length = AddressLength.Byte;
                        NewTag.Start = int.Parse(Regex.Match(address, @"MB[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(2));
                    }
                    else if (AddressType == 'W')
                    {
                        NewTag.Length = AddressLength.Word;
                        NewTag.Start = int.Parse(Regex.Match(address, @"MW[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(2));
                    }
                    else if (AddressType == 'D')
                    {
                        NewTag.Length = AddressLength.DWord;
                        NewTag.Start = int.Parse(Regex.Match(address, @"MD[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(2));
                    }
                }
            }
            else if (IRegex.IsMatch(address))
            {
                Match AddressTypeMatch = Regex.Match(address, "(IB|IW|ID)");
                if (!AddressTypeMatch.Success)
                {
                        NewTag.Length = AddressLength.Bit;
                        NewTag.Start = int.Parse(Regex.Match(address, @"I[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(1));
                        NewTag.Bit = int.Parse(Regex.Match(address, @"\.[0-7]").Value.Substring(1));
                }
                else
                {
                    char AddressType = AddressTypeMatch.Value[1];
                    if (AddressType == 'B')
                    {
                        NewTag.Length = AddressLength.Byte;
                        NewTag.Start = int.Parse(Regex.Match(address, @"IB[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(2));
                    }
                    else if (AddressType == 'W')
                    {
                        NewTag.Length = AddressLength.Word;
                        NewTag.Start = int.Parse(Regex.Match(address, @"IW[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(2));
                    }
                    else if (AddressType == 'D')
                    {
                        NewTag.Length = AddressLength.DWord;
                        NewTag.Start = int.Parse(Regex.Match(address, @"ID[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(2));
                    }
                }
            }
            else if (QRegex.IsMatch(address))
            {
                Match AddressTypeMatch = Regex.Match(address, "(QB|QW|QD)");
                if (!AddressTypeMatch.Success)
                {
                        NewTag.Length = AddressLength.Bit;
                        NewTag.Start = int.Parse(Regex.Match(address, @"Q[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(1));
                        NewTag.Bit = int.Parse(Regex.Match(address, @"\.[0-7]").Value.Substring(1));
                }
                else
                {
                    char AddressType = AddressTypeMatch.Value[1];
                    if (AddressType == 'B')
                    {
                        NewTag.Length = AddressLength.Byte;
                        NewTag.Start = int.Parse(Regex.Match(address, @"QB[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(2));
                    }
                    else if (AddressType == 'W')
                    {
                        NewTag.Length = AddressLength.Word;
                        NewTag.Start = int.Parse(Regex.Match(address, @"QW[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(2));
                    }
                    else if (AddressType == 'D')
                    {
                        NewTag.Length = AddressLength.DWord;
                        NewTag.Start = int.Parse(Regex.Match(address, @"QD[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(2));
                    }
                }
            }
            else if (CRegex.IsMatch(address))
            {
                NewTag.Length = AddressLength.Counter;
                NewTag.Start = int.Parse(Regex.Match(address, @"C[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(1));
            }
            else if (TRegex.IsMatch(address))
            {
                NewTag.Length = AddressLength.Timer;
                NewTag.Start = int.Parse(Regex.Match(address, @"T[0-9]+[0-9]*[0-9]*[0-9]*[0-9]*").Value.Substring(1));
            }
            else
            {
                return null;
            }
            return NewTag;
        }
    }
}
