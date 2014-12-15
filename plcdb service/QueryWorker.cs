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
        private QueryWorkerLogger Log = (QueryWorkerLogger)LogManager.GetCurrentClassLogger(typeof(QueryWorkerLogger));
        private Model ActiveModel { get; set; }
        private Model.QueriesRow ActiveQuery { get; set; }
        private Model.TagsRow TriggerTag = null;
        private ControllerBase TriggerController = null;
        
        public StatusEnum Status { get; set; }
        public bool StopRequest { get; set; }
        public long QueryPK
        {
            get
            {
                return ActiveQuery.PK;
            }
        }

        #region CTor
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
                Log.Error(this, e);
            }
        }
        #endregion

        public void DoWork()
        {
            Log.Info(this, "Worker thread beginning for Query PK " + QueryPK + " (" + ActiveQuery.Name + ")");
            if (ActiveModel == null || ActiveQuery == null)
            {
                Log.Error(this, "ActiveModel or ActiveQuery is null");
                return;
            }

            try
            {
                TriggerTag = ActiveModel.Tags.FindByPK(ActiveQuery.TriggerTag);
                TriggerController = TriggerTag.ControllersRow.Controller;
                Log.Debug(this, "Trigger tag and controller found");
            }
            catch (Exception e)
            {
                Status = StatusEnum.Error;
                Log.Error(this, "Error finding trigger tag or controller");
                Log.Error(this, e);
            }
            while (!StopRequest)
            {
                Log.Debug(this, "Starting event loop");
                try
                {
                    bool QueryTriggered = (bool)TriggerController.Read(TriggerTag);

                    if (QueryTriggered)
                    {
                        Log.Debug(this, "Query triggered");
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
                            case "STORED PROCEDURE":
                                ProcessStoredProcedure();
                                break;
                            default:
                                throw new KeyNotFoundException("Invalid query type: " + ActiveQuery.QueryType);
                        }
                        Log.Debug(this, "Query completed");
                    }
                    else
                    {
                        Log.Debug(this, "Query not triggered");
                    }
                    Status = StatusEnum.Good;
                }
                catch (Exception e)
                {
                    Log.Error(this, "Error during query execution or trigger tag read"); 
                    Status = StatusEnum.Error;
                    Log.Error(this, e);
                }
                
                Thread.Sleep((int)ActiveQuery.RefreshRate);

            }
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
            Log.TraceEnterFunction(this);
            SqlConnection sqlConnection = new SqlConnection(ActiveQuery.DatabasesRow.ConnectionString);
            SqlCommand sqlCommand = new SqlCommand("", sqlConnection);
            String InsertQueryStart = "INSERT INTO " + ActiveQuery.QueryText + " (";
            String InsertQueryEnd = " VALUES (";
            var TagMappings = ActiveModel.QueryTagMappings.Where(p => p.Query == ActiveQuery.PK);
            foreach (Model.QueryTagMappingsRow TagMapping in TagMappings)
            {
                if (TagMapping != null && !TagMapping.IsColumnNameNull() && TagMapping.TagsRow != null && TagMapping.QueriesRow != null)
                {
                    Log.Debug(this, "Mapping parameter to query: " + TagMapping.TagsRow + " to column " + TagMapping.ColumnName);
                    Model.TagsRow Tag = TagMapping.TagsRow;
                    ControllerBase Controller = Tag.ControllersRow.Controller;
                    InsertQueryStart += TagMapping.ColumnName + ", ";
                    String ValueRead = Controller.Read(TagMapping.TagsRow).ToSqlString() + ", ";
                    InsertQueryEnd += ValueRead;
                    Log.Info(this, "Read input parameter for column '" + TagMapping.ColumnName + " from Tag " + TagMapping.TagsRow + "'. Value Read=" + ValueRead);
                }
            }
            InsertQueryStart = InsertQueryStart.Remove(InsertQueryStart.Length-2) + ")";
            InsertQueryEnd = InsertQueryEnd.Remove(InsertQueryEnd.Length - 2) + ")";
            
            String InsertQuery = InsertQueryStart + InsertQueryEnd;
            Log.Info(this, "Query string assembled: " + InsertQuery);
            sqlCommand.CommandText = InsertQuery;
            sqlConnection.Open();
            sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
            Log.TraceExitFunction(this);
        }

        private void ProcessSelectQuery()
        {
            Log.TraceEnterFunction(this);
            SqlConnection sqlConnection = new SqlConnection(ActiveQuery.DatabasesRow.ConnectionString);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(ActiveQuery.QueryText, sqlConnection);

            DataTable dataResults = new DataTable();
            Log.Info(this, "Performing query: " + ActiveQuery.QueryText);
            sqlDataAdapter.Fill(dataResults);
            Log.Debug(this, "Query completed");
            if (dataResults.Rows.Count > 100)
            {
                Log.Warn("Warning! Query returned many results (" + dataResults.Rows.Count + " rows)!");
            }
            foreach (DataRow row in dataResults.Rows)
            {
                foreach (DataColumn col in dataResults.Columns)
                {
                    Model.QueryTagMappingsRow TagMapping = ActiveModel.QueryTagMappings.FirstOrDefault(p => p.Query == ActiveQuery.PK && p.ColumnName == col.ColumnName);
                    if (TagMapping != null && !TagMapping.IsColumnNameNull() && TagMapping.TagsRow != null && TagMapping.QueriesRow != null)
                    {
                        Log.Debug(this, "Mapping result to tag: " + TagMapping.ColumnName + " to tag " + TagMapping.TagsRow);
                        Model.TagsRow TagToWrite = TagMapping.TagsRow;
                        ControllerBase ControllerToWrite = TagToWrite.ControllersRow.Controller;
                        ControllerToWrite.Write(TagToWrite, row[col]);
                        Log.Info(this, "Wrote column '" + TagMapping.ColumnName + "', value '" + row[col] + "' to tag " + TagToWrite);
                    }
                    else
                    {
                        Log.Debug(this, "Query triggered but TagMapping not set up for column '" + col.ColumnName + "'");
                    }
                }
            }
            Log.TraceExitFunction(this);
        }

        private void ProcessStoredProcedure()
        {
            Log.TraceEnterFunction(this);
            Log.Info(this, "Beginning STORED PROCEDURE: " + ActiveQuery.QueryText);
            SqlConnection sqlConnection = new SqlConnection(ActiveQuery.DatabasesRow.ConnectionString);
            SqlCommand sqlCommand = new SqlCommand(ActiveQuery.QueryText, sqlConnection);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlCommand.CommandType = CommandType.StoredProcedure;

            foreach (Model.QueryTagMappingsRow tagMapping in ActiveQuery.GetQueryTagMappingsRows())
            {
                if (tagMapping != null && !tagMapping.IsColumnNameNull() && tagMapping.TagsRow != null && tagMapping.QueriesRow != null)
                {
                    Log.Debug(this, "Setting up tag mapping for column '" + tagMapping.ColumnName + "'");
                    SqlParameter Parameter = new SqlParameter(tagMapping.ColumnName, null);
                    if (tagMapping.ColumnName == "@RETURN_VALUE")
                    {
                        Parameter.Direction = ParameterDirection.ReturnValue;
                    }
                    else if (tagMapping.OutputToController)
                    {
                        Parameter.Direction = ParameterDirection.InputOutput;
                        Parameter.Value = tagMapping.TagsRow.ControllersRow.Controller.Read(tagMapping.TagsRow).ToSqlString();
                        Log.Info(this, "Read InOut parameter '" + Parameter.ParameterName + "' from tag " + tagMapping.TagsRow + ". Value read=" + Parameter.Value);
                    }
                    else
                    {
                        Parameter.Direction = ParameterDirection.Input;
                        Parameter.Value = tagMapping.TagsRow.ControllersRow.Controller.Read(tagMapping.TagsRow).ToSqlString();
                        Log.Info(this, "Read input parameter '" + Parameter.ParameterName + "' from tag " + tagMapping.TagsRow + ". Value read=" + Parameter.Value);
                    }
                    Log.Debug(this, "Parameterization complete for for column '" + tagMapping.ColumnName + "', type=" + Parameter.Direction.ToString());
                    sqlCommand.Parameters.Add(Parameter);
                }
                else
                {
                    Log.Debug(this, "Query triggered but TagMapping not set up for column '" + tagMapping.ColumnName + "'");
                }
            }
            
            sqlConnection.Open();

            DataTable dataResults = new DataTable();
            sqlDataAdapter.Fill(dataResults);
            Log.Debug(this, "Stored procedure execution completed");

            foreach (SqlParameter Parameter in sqlCommand.Parameters)
            {
                if (Parameter.Direction != ParameterDirection.Input)
                {
                    Log.Debug(this, "Mapping output parameter '" + Parameter.ParameterName + "'");
                    Model.QueryTagMappingsRow TagMapping = ActiveModel.QueryTagMappings.FirstOrDefault(p => p.Query == ActiveQuery.PK && p.ColumnName == Parameter.ParameterName);
                    if (TagMapping != null && !TagMapping.IsColumnNameNull() && TagMapping.TagsRow != null && TagMapping.QueriesRow != null)
                    {
                        Model.TagsRow TagToWrite = TagMapping.TagsRow;
                        ControllerBase ControllerToWrite = TagToWrite.ControllersRow.Controller;
                        ControllerToWrite.Write(TagToWrite, Parameter.Value);
                        Log.Info(this, "Wrote output parameter '" + Parameter.ParameterName + "', value '" + Parameter.Value.ToString() + "' to tag '" + TagToWrite + "'");
                    }
                    else
                    {
                        Log.Debug(this, "Query triggered but TagMapping not set up for column '" + Parameter.ParameterName + "'");
                    }
                    
                }
            }
            Log.TraceExitFunction(this);
        }

    }
}
