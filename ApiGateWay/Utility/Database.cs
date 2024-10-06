using System;
using Microsoft.Extensions.Logging;
using System.Data;
using Newtonsoft.Json;
using NLog;
using Microsoft.Data.SqlClient;

namespace ApiGateWay.Utility
{
    public class Database : AppSetting
    {
        public Logger logger = LogManager.GetCurrentClassLogger();
        const int maxRetry = 3;
        private string DBConnectionString;
        public Database()
        {
            DBConnectionString = Get("DBConnection");
        }
        public DataTable selectSP(string StoredProcedureName, Dictionary<string, object> data)
        {
            int retry = 0;
            DataTable dataTable = new DataTable();
            while (retry < maxRetry)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(DBConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = Cmd(data))
                        {
                            command.Connection = connection;
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = StoredProcedureName;
                            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                            {
                                adapter.Fill(dataTable);
                            }
                        }
                        connection.Close();
                        if (dataTable != null & dataTable.Rows.Count > 0)
                        {
                            return dataTable;
                        }
                    }
                }
                catch (SqlException sqlex) when (sqlex.Number == -2)
                {
                    TimeOutError(retry, StoredProcedureName, data);
                    retry++;
                }
                catch (SqlException ex)
                {
                    DBError(ex.Message, StoredProcedureName, data);
                }
            }
            return null;
        }
        public bool execSP(string StoredProcedureName, Dictionary<string, object> data)
        {
            int retry = 0;
            bool result = false;
            while (retry < maxRetry)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(DBConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(StoredProcedureName, connection))
                        {
                            command.Connection = connection;
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = StoredProcedureName;
                            if (command.ExecuteNonQuery() > 0)
                            {
                                result = true;
                            }
                        }
                        connection.Close();
                    }
                }
                catch (SqlException sqlex) when (sqlex.Number == -2)
                {
                    TimeOutError(retry, StoredProcedureName, data);
                    retry++;
                }
                catch (SqlException ex)
                {
                    DBError(ex.Message, StoredProcedureName, data);
                }
            }
            return result;
        }
        public List<DataTable> selectSPs(string StoredProcedureName, Dictionary<string, object> data)
        {
            int retry = 0;
            List<DataTable> dataTables = new List<DataTable>();
            while (retry < maxRetry)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(DBConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = Cmd(data))
                        {
                            command.Connection = connection;
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = StoredProcedureName;
                            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                            {
                                DataSet dataSet = new DataSet();
                                adapter.Fill(dataSet);
                                foreach (DataTable table in dataSet.Tables)
                                {
                                    dataTables.Add(table);
                                }
                            }
                        }
                        connection.Close();
                        return dataTables;
                    }
                }
                catch (SqlException sqlex) when (sqlex.Number == -2)
                {
                    TimeOutError(retry, StoredProcedureName, data);
                    retry++;
                }
                catch (SqlException ex)
                {
                    DBError(ex.Message, StoredProcedureName, data);
                }
            }
            return null;
        }
        private SqlCommand Cmd(Dictionary<string, object> data)
        {
            SqlCommand command = new SqlCommand();
            foreach (KeyValuePair<string, object> parameterName in data)
            {
                command.Parameters.AddWithValue("@" + parameterName.Key, parameterName.Value);
            }
            return command;
        }
        private void TimeOutError(int retry, string StoredProcedureName, Dictionary<string, object> data)
        {
            logger.Error($"sqlSP Retry {retry} TimeOut:{JsonConvert.SerializeObject(new { StoredProcedure = StoredProcedureName, Data = JsonConvert.SerializeObject(data) })}");
        }
        private void DBError(string error, string StoredProcedureName, Dictionary<string, object> data)
        {
            logger.Error($"sqlSP:{JsonConvert.SerializeObject(new { StoredProcedure = StoredProcedureName, Data = JsonConvert.SerializeObject(data), Error = error })}");
        }
    }
}

