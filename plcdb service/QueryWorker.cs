using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using plcdb_lib.Models;
using plcdb_lib.Models.Controllers;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using NLog;
using plcdb_lib.SQL;
using plcdb_lib.WCF;
using System.Diagnostics;

namespace plcdb_service
{
    class QueryWorker
    {
        private Logger Log = LogManager.GetCurrentClassLogger();
        public Model ActiveModel { get; set; }
        public Model.QueriesRow ActiveQuery { get; set; }
        public bool StopRequest {get;set;}
        private Model.TagsRow TriggerTag = null;
        private ControllerBase TriggerController = null;
        private static object queryLock = new object();
        public StatusEnum Status { get; set; }

        public QueryWorker(Model m, Model.QueriesRow q)
        {
            try
            {
                ActiveModel = (Model)m.Copy();
                ActiveQuery = ActiveModel.Queries.First(p => p.PK == q.PK);
                TriggerController = m.Tags.First(p => p.PK == q.TriggerTag).ControllersRow.Controller;
                Status = StatusEnum.Good;
            }
            catch (Exception e)
            {
                Status = StatusEnum.Error;
                Log.Log(Error(e.Message));
            }
        }

        public void DoWork()
        {
            
            if (ActiveModel == null || ActiveQuery == null)
            {
                Log.Log(Error("ActiveModel or ActiveQuery is null"));
                return;
            }

            try
            {
                TriggerTag = ActiveModel.Tags.FindByPK(ActiveQuery.TriggerTag);
                TriggerController = TriggerTag.ControllersRow.Controller;
            }
            catch (Exception e)
            {
                Status = StatusEnum.Error;
                Log.Error(Error(e.Message));
            }
            while (!StopRequest)
            {
                try
                {
                    //lock (queryLock)
                    //{
                    bool QueryTriggered = (bool)TriggerController.Read(TriggerTag);

                    if (QueryTriggered)
                    {
                        
                        switch (ActiveQuery.QueryType)
                        {
                            case "SELECT":
                                ProcessSelectQuery();
                                break;
                            case "INSERT":
                                ProcessInsertQuery();
                                break;
                            case "UPDATE":
                                ProcessUpdateQuery();
                                break;
                            case "DELETE":
                                ProcessDeleteQuery();
                                break;
                        }
                    }
                    Status = StatusEnum.Good;
                }
                catch (Exception e)
                {
                    Status = StatusEnum.Error;
                    Log.Log(Error(e.Message));
                }
                
                Thread.Sleep((int)ActiveQuery.RefreshRate);

            }
        }

        private LogEventInfo Error(string Message)
        {
            LogEventInfo LogEvent = new LogEventInfo()
            {
                Message = Message,
                Level = LogLevel.Error,
                TimeStamp = DateTime.Now,
                LoggerName = "Query Worker",
            };
            LogEvent.Properties["Query"] = ActiveQuery.PK;
            return LogEvent;
        }

        private void ProcessDeleteQuery()
        {
            throw new NotImplementedException();
        }

        private void ProcessUpdateQuery()
        {
            throw new NotImplementedException();
        }

        private void ProcessInsertQuery()
        {
            SqlConnection sqlConnection = new SqlConnection(ActiveQuery.DatabasesRow.ConnectionString);
            SqlCommand sqlCommand = new SqlCommand("", sqlConnection);
            String InsertQueryStart = "INSERT INTO " + ActiveQuery.QueryText + " (";
            String InsertQueryEnd = " VALUES (";
            var TagMappings = ActiveModel.QueryTagMappings.Where(p => p.Query == ActiveQuery.PK);
            foreach (Model.QueryTagMappingsRow TagMapping in TagMappings)
            {
                if (TagMapping != null && !TagMapping.IsColumnNameNull() && TagMapping.TagsRow != null && TagMapping.QueriesRow != null)
                {
                    Model.TagsRow Tag = TagMapping.TagsRow;
                    ControllerBase Controller = Tag.ControllersRow.Controller;
                    InsertQueryStart += TagMapping.ColumnName + ", ";
                    InsertQueryEnd += Controller.Read(TagMapping.TagsRow).ToSqlString() + ", ";
                }
            }
            InsertQueryStart = InsertQueryStart.Remove(InsertQueryStart.Length-2) + ")";
            InsertQueryEnd = InsertQueryEnd.Remove(InsertQueryEnd.Length - 2) + ")";
            
            String InsertQuery = InsertQueryStart + InsertQueryEnd;
            sqlCommand.CommandText = InsertQuery;
            sqlConnection.Open();
            sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
        }

        private void ProcessSelectQuery()
        {
            SqlConnection sqlConnection = new SqlConnection(ActiveQuery.DatabasesRow.ConnectionString);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(ActiveQuery.QueryText, sqlConnection);

            DataTable dataResults = new DataTable();
            sqlDataAdapter.Fill(dataResults);

            foreach (DataRow row in dataResults.Rows)
            {
                foreach (DataColumn col in dataResults.Columns)
                {
                    Model.QueryTagMappingsRow TagMapping = ActiveModel.QueryTagMappings.FirstOrDefault(p => p.Query == ActiveQuery.PK && p.ColumnName == col.ColumnName);
                    if (TagMapping != null && !TagMapping.IsColumnNameNull() && TagMapping.TagsRow != null && TagMapping.QueriesRow != null)
                    {
                        Model.TagsRow TagToWrite = TagMapping.TagsRow;
                        ControllerBase ControllerToWrite = TagToWrite.ControllersRow.Controller;
                        ControllerToWrite.Write(TagToWrite, row[col]);
                    }
                    else
                    {
                        Log.Trace("Query triggered but TagMapping not set up for column '" + col.ColumnName + "'");
                    }
                }
            }
        }
    }
}
