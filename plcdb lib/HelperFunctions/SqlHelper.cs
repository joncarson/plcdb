using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace plcdb_lib.SQL
{
    public static class SqlHelper
    {
        
        public static String ToSqlString(this Object obj)
        {
            if (obj is String)
                return "'" + obj.ToString() + "'";

            else if (obj is Boolean)
                return String.Compare(obj.ToString(), "true", true) == 0 ? "1" : "0";

            else if (obj is DateTime)
                return "'" + ((DateTime)obj).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            
            return obj.ToString();
        }

        public static bool TestForServer(string address, int port = 1443)
        {
            if (address.ToLower() == "localhost" || address == "127.0.0.1")
                return true;

            int timeout = 500;
           
            var result = false;
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    IAsyncResult asyncResult = socket.BeginConnect(address, port, null, null);
                    result = asyncResult.AsyncWaitHandle.WaitOne(timeout, true);
                    socket.Close();
                }
                return result;
            }
            catch { return false; }
        }

        public static List<String> GetAllDatabasesInServer(String ConnectionString)
        {
            List<string> list = new List<string>();
            try
            {
                if (!SqlHelper.TestForServer(new SqlConnection(ConnectionString).DataSource))
                {
                    return list;
                }

                using (SqlConnection con = new SqlConnection(ConnectionString + "; Connection Timeout=1"))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
                    {
                        cmd.CommandTimeout = 500;
                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                list.Add(dr[0].ToString());
                            }
                        }
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                return list;
            }
        }

        public static bool TestConnectionString(String ConnectionString)
        {
            try
            {
                if (!SqlHelper.TestForServer(new SqlConnection(ConnectionString).DataSource))
                {
                    return false;
                }

                using (SqlConnection con = new SqlConnection(ConnectionString + "; Connection Timeout=1"))
                {
                    con.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
