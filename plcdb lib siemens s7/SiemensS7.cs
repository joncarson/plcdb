using plcdb_lib.Models.Controllers;
using Snap7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using plcdb_lib.Models;
using System.Timers;
using NLog;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace plcdb_lib_siemens_s7
{
    public class SiemensS7 : ControllerBase
    {
        Model.ControllersRow ControllerInfo;
        private static S7Client Client = new S7Client();
        private static List<SiemensTag> ActiveTags = new List<SiemensTag>();
        private static byte[] Buffer = new byte[1000000];
        Timer RefreshDataTimer = new Timer();
        private Logger Log = LogManager.GetCurrentClassLogger();
        BackgroundWorker bgReadThread = new BackgroundWorker();

        public static string Name
        {
            get { return "Siemens S7"; }
        }

        public SiemensS7(Model.ControllersRow ControllerInfo)
        {
            this.ControllerInfo = ControllerInfo;
        }

        void RefreshDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!bgReadThread.IsBusy)
                bgReadThread.RunWorkerAsync();
        }

        #region ControllerBase Methods
        public override Model.TagsDataTable BrowseTags()
        {
            return new Model.TagsDataTable();
        }

        public override bool ValidateTag(string address)
        {
            bool CreatedTagSuccess = SiemensTag.CreateTag(address) != null;
            return CreatedTagSuccess;
        }

        public override object Read(plcdb_lib.Models.Model.TagsRow t)
        {
            if (!Client.Connected())
            {
                int connResult = Client.ConnectTo(ControllerInfo.Address, (int)ControllerInfo.s7_rack, (int)ControllerInfo.s7_slot);
                if (connResult != 0)
                {
                    Log.Error(GetErrorText(connResult));
                    Client.Disconnect();
                    Client = null;
                    Client = new S7Client();
                    throw new EntryPointNotFoundException("Unable to connect to PLC " + ControllerInfo.Name);
                }
                bgReadThread.DoWork -= ReadFromPLC;
                bgReadThread.DoWork += ReadFromPLC;
                RefreshDataTimer.Interval = 1000;
                RefreshDataTimer.Elapsed -= RefreshDataTimer_Elapsed;
                RefreshDataTimer.Elapsed += RefreshDataTimer_Elapsed;
                RefreshDataTimer.Start();
            }

            SiemensTag Tag = ActiveTags.FirstOrDefault(p => p.TagRow.PK == t.PK);
            if (Tag == null)
            {
                Tag = new SiemensTag() { TagRow = t };
                ActiveTags.Add(Tag);
            }
            return Tag.TagRow.CurrentValue;
        }
        public override bool Write(plcdb_lib.Models.Model.TagsRow t, object val)
        {
            SiemensTag Tag = ActiveTags.FirstOrDefault(p => p.TagRow.PK == t.PK);
            if (Tag == null)
            {
                Tag = new SiemensTag() { TagRow = t };
                ActiveTags.Add(Tag);
            }
            WriteToPLC(Tag, val);
            return true;
        }
        #endregion

        #region PLC Methods
        private void WriteToPLC(SiemensTag t, object val)
        {
            lock (Client)
            {
                SiemensTag Tag = ActiveTags.FirstOrDefault(p => p.TagRow.PK == t.TagRow.PK);
                if (Tag == null)
                {
                    ActiveTags.Add(new SiemensTag()
                    {
                        TagRow = t.TagRow
                    });
                }
                int result = 0;
                if (Tag.Length == SiemensTag.AddressLength.Bit) //if bit is used, start address is byte_number*8 + bit_number
                {
                    byte mask = (byte)(1 << Tag.Bit);
                    if (Convert.ToBoolean(val)) Buffer[Tag.Start] |= mask;
                    else Buffer[Tag.Start] &= mask;
                    result = Client.WriteArea((int)Tag.AddressSpace, Tag.DB, Tag.Start * 8 + Tag.Bit, 1, (int)Tag.Length, Buffer);
                }
                else if (Tag.Length == SiemensTag.AddressLength.DWord || Tag.Length == SiemensTag.AddressLength.Real)
                {
                    byte[] ConvertedToBytes = BitConverter.GetBytes((int)val);
                    for (int i = 0; i < 4; i++)
                    {
                        Buffer[Tag.Start + i] = ConvertedToBytes[i];
                    }
                    result = Client.WriteArea((int)Tag.AddressSpace, Tag.DB, Tag.Start, 2, (int)Tag.Length, Buffer);
                }
                else
                {
                    byte[] ConvertedToBytes = BitConverter.GetBytes(short.Parse(val.ToString()));
                    for (int i = 0; i < 2; i++)
                    {
                        Buffer[Tag.Start + i] = ConvertedToBytes[i];
                    }
                    result = Client.WriteArea((int)Tag.AddressSpace, Tag.DB, Tag.Start, 1, (int)Tag.Length, Buffer);
                }
                if (result != 0)
                {
                    Log.Error("Error writing tag " + Tag.TagRow + ", " + GetErrorText(result));
                    Client.Disconnect();
                    Client = null;
                    Client = new S7Client();
                }
            }
        }

        private void ReadFromPLC(Object sender, EventArgs e)
        {
            try
            {
                lock (Client)
                {
                    if (!Client.Connected())
                    {
                        int connResult = Client.ConnectTo(ControllerInfo.Address, (int)ControllerInfo.s7_rack, (int)ControllerInfo.s7_slot);
                        if (connResult != 0)
                        {
                            Log.Error(GetErrorText(connResult));
                            Client.Disconnect();
                            Client = null;
                            Client = new S7Client();
                            return;
                        }
                    }

                    foreach (SiemensTag Tag in ActiveTags)
                    {
                        int result = 0;
                        if (Tag.Length == SiemensTag.AddressLength.Bit) //if bit is used, start address is byte_number*8 + bit_number
                        {
                            result = Client.WriteArea((int)Tag.AddressSpace, Tag.DB, Tag.Start * 8 + Tag.Bit, 1, (int)Tag.Length, Buffer);
                        }
                        else if (Tag.Length == SiemensTag.AddressLength.DWord || Tag.Length == SiemensTag.AddressLength.Real)
                        {
                            result = Client.ReadArea((int)Tag.AddressSpace, Tag.DB, Tag.Start, 2, (int)Tag.Length, Buffer);
                        }
                        else
                        {
                            result = Client.ReadArea((int)Tag.AddressSpace, Tag.DB, Tag.Start, 1, (int)Tag.Length, Buffer);
                        }
                        if (result != 0)
                        {
                            Log.Error("Error reading tag " + Tag.TagRow + ", " + GetErrorText(result));
                            Client.Disconnect();
                            return;
                        }
                        else
                        {
                            if (Tag.Length == SiemensTag.AddressLength.Bit)
                                Tag.TagRow.CurrentValue = (Buffer[Tag.Start] & (1 << Tag.Bit)) != 0;
                            else if (Tag.Length == SiemensTag.AddressLength.Byte)
                                Tag.TagRow.CurrentValue = Buffer[Tag.Start];
                            else if (Tag.Length == SiemensTag.AddressLength.Word)
                                Tag.TagRow.CurrentValue = BitConverter.ToInt16(Buffer, Tag.Start);
                            else if (Tag.Length == SiemensTag.AddressLength.Real || Tag.Length == SiemensTag.AddressLength.DWord)
                                Tag.TagRow.CurrentValue = BitConverter.ToInt32(Buffer, Tag.Start);
                            else
                                throw new MissingMemberException("Unknown type in Siemens tag: " + Tag.TagRow);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error reading from PLC: " + ControllerInfo.Name + ". Reconnecting.");
            }
        }

        private String GetErrorText(int result)
        {
            if (result < 0)
                return "S7 Library Error";
            else
                return Client.ErrorText(result);
        }
        #endregion

    }
}
