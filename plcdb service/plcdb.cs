using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using plcdb_lib.Models;
using System.Data.SqlClient;
using System.Threading;

namespace plcdb_service
{
    public partial class plcdb : ServiceBase
    {
        private Model ActiveModel = new Model();
        List<SqlConnection> SqlConnections = new List<SqlConnection>();
        List<Thread> ActiveThreads = new List<Thread>();

        public plcdb()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //Load model
            ActiveModel.Open(Properties.Settings.Default.ConfigFile);

            //Load Sql connections
            foreach (Model.DatabasesRow row in ActiveModel.Databases)
            {
                SqlConnection conn = new SqlConnection(row.ConnectionString);
                conn.Open();
                SqlConnections.Add(conn);
            }
        }

        protected override void OnStop()
        {

        }
    }
}
