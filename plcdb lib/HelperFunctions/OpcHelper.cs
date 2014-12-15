using ProcessControlStandards.OPC.Core;
using ProcessControlStandards.OPC.DataAccessClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace plcdb_lib.HelperFunctions
{
    public static class OpcHelper
    {
        public static List<string> GetOpcServers(string host)
        {
            List<String> OpcServers = new List<string>();
            try
            {
                ServerBrowser browser = new ServerBrowser(host);
                
                foreach (ServerDescription OpcServer in browser.GetEnumerator(DAServer.Version10, DAServer.Version20))
                {
                    OpcServers.Add(OpcServer.ProgramId);
                }
                return OpcServers;
            }
               catch (Exception e)
            {
                return OpcServers;
            }
        }
    }
}
