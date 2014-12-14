using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using plcdb.Helpers;
using plcdb_lib.Models;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace plcdb.ViewModels
{
    public class QueryPopupViewModel : BaseViewModel
    {
        private Model ActiveModel = (App.Current.Resources["MainWindowViewModel"] as MainWindowViewModel).ActiveModel;

        #region Properties

        #region AvailableDatabases
        private DataRowCollection _availableDatabases;
        public DataRowCollection AvailableDatabases
        {
            get
            {
                if (_availableDatabases == null)
                {
                    _availableDatabases = ActiveModel.Databases.Rows;
                }
                return _availableDatabases;
            }
            set
            {
                _availableDatabases = value;
                RaisePropertyChanged(() => AvailableDatabases);
            }
        }
        #endregion

        #region AvailableTables
        private List<String> _availableTables;
        public List<String> AvailableTables
        {
            get
            {
                if (_availableTables == null)
                {
                    RefreshTables();
                }
                return _availableTables;
            }
            set
            {
                _availableTables = value;
                RaisePropertyChanged(() => AvailableTables);
            }
        }

        
        #endregion

        #region AvailableStoredProcedures
        private List<String> _availableStoredProcedures;
        public List<String> AvailableStoredProcedures
        {
            get
            {
                if (_availableStoredProcedures == null)
                {
                    RefreshStoredProcedures();
                }
                return _availableStoredProcedures;
            }
            set
            {
                _availableStoredProcedures = value;
                RaisePropertyChanged(() => AvailableStoredProcedures);
            }
        }

        #endregion

        #region AvailableQueryTypes
        private List<String> _availableQueryTypes;
        public List<String> AvailableQueryTypes
        {
            get
            {
                if (_availableQueryTypes == null)
                {
                    List<String> lst = new List<string>();
                    lst.Add("SELECT");
                    lst.Add("INSERT");
                    lst.Add("STORED PROCEDURE");
                    lst.Add("UPDATE");
                    lst.Add("DELETE");
                    _availableQueryTypes = lst;
                }
                return _availableQueryTypes;
            }
            set
            {
                _availableQueryTypes = value;
                RaisePropertyChanged(() => AvailableQueryTypes);
            }
        }
        #endregion

        #region CurrentQuery
        private plcdb_lib.Models.Model.QueriesRow _currentQuery;
        public plcdb_lib.Models.Model.QueriesRow CurrentQuery
        {
            get
            {
                return _currentQuery;
            }
            set
            {
                if (_currentQuery != value)
                {
                    _currentQuery = value;
                    RaisePropertyChanged(() => CurrentQuery);
                }
                
                try
                {
                    Name = value.Name;
                    QueryType = value.QueryType;
                    QueryText = value.QueryText;
                    if (!value.IsDatabaseNull())
                        Database = value.DatabasesRow;
                    if (!value.IsTriggerTagNull())
                        TriggerTag = ActiveModel.Tags.FindByPK(value.TriggerTag);
                    RefreshRate = value.RefreshRate;
                    MappingType = value.MappingType;
                    MaxRows = value.MaxRows;
                }
                catch (Exception e)
                {
                    Name = "";
                    QueryType = "";
                    QueryText = "";
                    RefreshRate = 5000;
                    MappingType = "";
                    MaxRows = 1;
                }
            }
        }
        #endregion

        #region CurrentQuery
        private plcdb_lib.Models.Model.QueryTagMappingsRow _currentTagMapping;
        public plcdb_lib.Models.Model.QueryTagMappingsRow CurrentTagMapping
        {
            get
            {
                return _currentTagMapping;
            }
            set
            {
                if (_currentTagMapping != value)
                {
                    _currentTagMapping = value;
                    RaisePropertyChanged(() => CurrentTagMapping);
                    RaisePropertyChanged(() => TagMappings);
                }

            }
        }
        #endregion

        #region Name
        public String Name
        {
            get { return CurrentQuery.Name; }
            set
            {
                if (CurrentQuery.Name != value)
                {
                    CurrentQuery.Name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }
        #endregion

        #region QueryType
        public string QueryType
        {
            get { return CurrentQuery.QueryType; }
            set
            {
                if (CurrentQuery.QueryType != null && CurrentQuery.QueryType != value)
                {
                    CurrentQuery.QueryType = value;
                    RaisePropertyChanged(() => QueryType);
                }
            }
        }
        #endregion
         
        #region QueryText
        public string QueryText
        {
            get { return CurrentQuery.QueryText; }
            set
            {
                if (CurrentQuery.QueryText != null && CurrentQuery.QueryText != value)
                {
                    CurrentQuery.QueryText = value;
                    RaisePropertyChanged(() => QueryText);
                }
            }
        }
        #endregion

        #region TriggerTag
        public Model.TagsRow TriggerTag
        {
            get 
            {
                if (CurrentQuery.IsTriggerTagNull())
                    return null;
                var vm = App.Current.Resources["MainWindowViewModel"] as MainWindowViewModel;
                return vm.ActiveModel.Tags.FindByPK(CurrentQuery.TriggerTag);
            }
            set
            {
                if (value == null)
                {
                    CurrentQuery.TriggerTag = 0;
                }
                else
                {
                    if (CurrentQuery.IsTriggerTagNull() || CurrentQuery.TriggerTag != value.PK)
                    {
                        CurrentQuery.TriggerTag = value.PK;
                        RaisePropertyChanged(() => TriggerTag);
                    }
                }
            }
        }
        #endregion

        #region Database
        public Model.DatabasesRow Database
        {
            get 
            {
                if (CurrentQuery.IsDatabaseNull())
                    return null;
                return CurrentQuery.DatabasesRow;
            }
            set
            {
                if (CurrentQuery.IsDatabaseNull() || CurrentQuery.DatabasesRow != value)
                {
                    CurrentQuery.DatabasesRow = value;
                    CurrentQuery.Database = value.PK;
                    RaisePropertyChanged(() => Database);
                    RefreshStoredProcedures();
                    RefreshTables();
                }
            }
        }
        #endregion

        #region RefreshRate
        public long RefreshRate
        {
            get { return CurrentQuery.RefreshRate; }
            set
            {
                if (CurrentQuery.RefreshRate != null && CurrentQuery.RefreshRate != value)
                {
                    CurrentQuery.RefreshRate = value;
                    RaisePropertyChanged(() => RefreshRate);
                }
            }
        }
        #endregion

        #region AvailableMappingTypes
        private List<String> _availableMappingTypes;
        public List<String> AvailableMappingTypes
        {
            get
            {
                if (_availableMappingTypes == null)
                {
                    _availableMappingTypes = new List<String>();
                    _availableMappingTypes.Add("Single Row");
                    _availableMappingTypes.Add("Multi-Row (array)");
                    _availableMappingTypes.Add("Multi-Row (manual");
                }
                return _availableMappingTypes;
            }
        }
        #endregion

        #region MappingType
        public string MappingType
        {
            get { return CurrentQuery.MappingType; }
            set
            {
                if (CurrentQuery.IsMappingTypeNull() || CurrentQuery.MappingType != value)
                {
                    CurrentQuery.MappingType = value;
                    RaisePropertyChanged(() => MappingType);
                }
            }
        }
        #endregion

        #region MaxRows
        public long MaxRows
        {
            get { return CurrentQuery.MaxRows; }
            set
            {
                if (CurrentQuery.MaxRows != null && CurrentQuery.MaxRows != value)
                {
                    CurrentQuery.MaxRows = value;
                    RaisePropertyChanged(() => MaxRows);
                }
            }
        }
        #endregion

        #region TagMappings
        public Model.QueryTagMappingsRow[] TagMappings
        {
            get
            {
                return CurrentQuery.GetQueryTagMappingsRows();
            }
            set
            {
                ActiveModel.QueryTagMappings.AcceptChanges();
                foreach (Model.QueryTagMappingsRow row in ActiveModel.QueryTagMappings.Where(p => p.Query == CurrentQuery.PK)) 
                {
                    row.Delete();
                    //ActiveModel.QueryTagMappings.Rows.Remove(row);
                }
                foreach (Model.QueryTagMappingsRow row in value)
                {
                    row.Query = CurrentQuery.PK;
                    ActiveModel.QueryTagMappings.Rows.Add(row.ItemArray);
                }
                ActiveModel.QueryTagMappings.AcceptChanges();
                RaisePropertyChanged(() => TagMappings);
            }
        }
        #endregion

        #endregion


        #region Commands

        public ICommand SaveCommand { get { return new DelegateCommand(OnSave); } }
        public ICommand CancelCommand { get { return new DelegateCommand(OnCancel); } }
        public ICommand RefreshColumnsCommand { get { return new DelegateCommand(OnRefreshColumns); } }
        
        #endregion

        #region Ctor
        public QueryPopupViewModel()
        {
            
        }
        
        #endregion

        #region Command Handlers

        private void OnSave()
        {
            CurrentQuery.AcceptChanges();
            ActiveModel.QueryTagMappings.AcceptChanges();
        }

        private void OnCancel()
        {
            CurrentQuery.RejectChanges();
            ActiveModel.QueryTagMappings.RejectChanges();
        }

        private void OnRefreshColumns()
        {
            try
            {
                if (QueryType == "SELECT")
                    RefreshColumnsSelect(QueryText);
                else if (QueryType == "INSERT")
                    RefreshColumnsInsert("SELECT * FROM " + QueryText);
                else if (QueryType == "STORED PROCEDURE")
                    RefreshStoredProcColumns(QueryText);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            
        }

        private void RefreshStoredProcColumns(string QueryText)
        {
            List<Model.QueryTagMappingsRow> Columns = new List<Model.QueryTagMappingsRow>();
            SqlConnection conn = new SqlConnection(CurrentQuery.DatabasesRow.ConnectionString);
            SqlCommand cmd = new SqlCommand(QueryText, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();
            SqlCommandBuilder.DeriveParameters(cmd);
            foreach (SqlParameter p in cmd.Parameters)
            {
                Model.QueryTagMappingsRow NewRow = ActiveModel.QueryTagMappings.NewQueryTagMappingsRow();
                NewRow.ColumnName = p.ParameterName;
                NewRow.Query = CurrentQuery.PK;
                NewRow.Tag = 0;
                if (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue || p.Direction == ParameterDirection.InputOutput)
                    NewRow.OutputToController = true;

                Columns.Add(NewRow);
            }
            TagMappings = Columns.ToArray();
        }
        private void RefreshColumnsInsert(string QueryText)
        {
            List<Model.QueryTagMappingsRow> Columns = new List<Model.QueryTagMappingsRow>();
            SqlConnection Connection = new SqlConnection(CurrentQuery.DatabasesRow.ConnectionString);
            String StrippedQueryString = QueryText;
            foreach (Match match in Regex.Matches(QueryText, "(@[_a-zA-Z]+)"))
            {
                StrippedQueryString = StrippedQueryString.Replace(match.Value, "''");
            }

            SqlCommand QueryCommand = new SqlCommand(StrippedQueryString, Connection);
            Connection.Open();
            SqlDataReader sqlReader = QueryCommand.ExecuteReader(CommandBehavior.SchemaOnly);

            DataTable Results = sqlReader.GetSchemaTable();

            foreach (DataRow row in Results.Rows)
            {
                if (!(bool)row["IsReadOnly"])
                {
                    Model.QueryTagMappingsRow NewRow = ActiveModel.QueryTagMappings.NewQueryTagMappingsRow();
                    NewRow.ColumnName = row["ColumnName"].ToString();
                    NewRow.Query = CurrentQuery.PK;
                    NewRow.Tag = 0;
                    NewRow.OutputToController = false;
                    Columns.Add(NewRow);
                }
            }
            foreach (Match match in Regex.Matches(QueryText, "(@[_a-zA-Z]+)"))
            {
                Model.QueryTagMappingsRow NewRow = ActiveModel.QueryTagMappings.NewQueryTagMappingsRow();
                NewRow.ColumnName = match.Value;
                NewRow.Query = CurrentQuery.PK;
                NewRow.Tag = 0;
                NewRow.OutputToController = false;
                Columns.Add(NewRow);
            }
            TagMappings = Columns.ToArray();

        }

        private void RefreshColumnsSelect(string QueryText)
        {
            List<Model.QueryTagMappingsRow> Columns = new List<Model.QueryTagMappingsRow>();
            SqlConnection Connection = new SqlConnection(CurrentQuery.DatabasesRow.ConnectionString);
            String StrippedQueryString = QueryText;
            foreach (Match match in Regex.Matches(QueryText, "(@[_a-zA-Z]+)"))
            {
                StrippedQueryString = StrippedQueryString.Replace(match.Value, "''");
            }

            SqlCommand QueryCommand = new SqlCommand(StrippedQueryString, Connection);
            Connection.Open();
            SqlDataReader sqlReader = QueryCommand.ExecuteReader(CommandBehavior.SchemaOnly);

            DataTable Results = sqlReader.GetSchemaTable();

            foreach (DataRow row in Results.Rows)
            {
                Model.QueryTagMappingsRow NewRow = ActiveModel.QueryTagMappings.NewQueryTagMappingsRow();
                NewRow.ColumnName = row["ColumnName"].ToString();
                NewRow.Query = CurrentQuery.PK;
                NewRow.Tag = 0;
                NewRow.OutputToController = true;
                Columns.Add(NewRow);
            }
            foreach (Match match in Regex.Matches(QueryText, "(@[_a-zA-Z]+)"))
            {
                Model.QueryTagMappingsRow NewRow = ActiveModel.QueryTagMappings.NewQueryTagMappingsRow();
                NewRow.ColumnName = match.Value;
                NewRow.Query = CurrentQuery.PK;
                NewRow.Tag = 0;
                NewRow.OutputToController = false;
                Columns.Add(NewRow);
            }
            TagMappings = Columns.ToArray();

        }

        private void RefreshTables()
        {
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (s, e) =>
            {
                try
                {
                    SqlConnection conn = new SqlConnection(CurrentQuery.DatabasesRow.ConnectionString);
                    conn.Open();
                    List<String> availableTables = new List<string>();
                    foreach (DataRow row in conn.GetSchema("Tables").Rows)
                    {
                        availableTables.Add((String)row["TABLE_NAME"]);
                    }
                    e.Result = availableTables;
                }
                catch (Exception ex)
                {
                    e.Result = new List<String>();
                }
            };

            bgWorker.RunWorkerCompleted += (s, e) =>
                {
                    AvailableTables = (List<String>)e.Result;
                };
            bgWorker.RunWorkerAsync();
        }
        private void RefreshStoredProcedures()
        {
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (s, e) =>
            {
                try
                {
                    SqlConnection conn = new SqlConnection(CurrentQuery.DatabasesRow.ConnectionString);
                    SqlDataAdapter QueryCommand = new SqlDataAdapter("select * from " + conn.Database + ".information_schema.routines  where routine_type = 'PROCEDURE'", conn);
                    conn.Open();
                    List<String> availableTables = new List<string>();
                    DataTable Procedures = new DataTable();
                    QueryCommand.Fill(Procedures);
                    foreach (DataRow row in Procedures.Rows)
                    {
                        availableTables.Add((String)row["SPECIFIC_NAME"]);
                    }
                    e.Result = availableTables;
                }
                catch (Exception ex)
                {
                    e.Result = new List<String>();
                }
            };

            bgWorker.RunWorkerCompleted += (s, e) =>
            {
                AvailableStoredProcedures = (List<String>)e.Result;
            };
            bgWorker.RunWorkerAsync();
        }
        #endregion

    }
}