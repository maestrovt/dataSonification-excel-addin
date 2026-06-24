using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace DataSonificationConfiguration
{
    public class DataSonificationDB
    {
        private SQLiteConnection connection;
        public DataSonificationDB(string file)
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = file;

            connection = new SQLiteConnection(builder.ConnectionString);
            connection.Open();
        }

        public int GetSID(string ticker)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT s_id from sonification where ticker=\"" + ticker + "\"";

            object ret = command.ExecuteScalar();
            if (ret == null)
                throw new ArgumentException("Invalid Ticker: " + ticker);
            return (int)((decimal)ret);
        }

        public int GetFirstId(string tableName, string idName)
        {
            const string DBNextIdQuery = "SELECT {1} from {0} order by {1} asc limit 1";

            var command = connection.CreateCommand();
            command.CommandText = string.Format(DBNextIdQuery, tableName, idName);

            int id = (int)(decimal)command.ExecuteScalar();
            return id;
        }

        public string GetAnalyzer(int sid)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT analyzer from sonification where s_id=" + sid;

            object ret = command.ExecuteScalar();
            return (string)ret;
        }

        public string GetArranger(int sid)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT arranger from sonification where s_id=" + sid;

            object ret = command.ExecuteScalar();
            return (string)ret;
        }

        public void DeleteDBObject(string tableName, string idName, object id)
        {
            const string DBDeleteQuery = "DELETE from {0} where {1}={2}";

            var command = connection.CreateCommand();
            command.CommandText = string.Format(DBDeleteQuery, tableName, idName, id);
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected != 1)
                System.Diagnostics.Debug.Assert(false);
        }

        // Gets a blank object in the specified table creating the entry in the database
        public Dictionary<string, object> GetDBObject(string tableName, string idName, Dictionary<string, object> defaultValues)
        {

            object id = -1;
            if (!defaultValues.TryGetValue(idName, out id))
            {
                id = GetNextId(tableName, idName);
                defaultValues[idName] = id;
            }
            int actualId = (int)id;

            // Delete any existing
            const string DBDeleteQuery = "DELETE from {0} where {1}={2}";

            var command = connection.CreateCommand();
            command.CommandText = string.Format(DBDeleteQuery, tableName, idName, actualId);
            command.ExecuteNonQuery();

            const string DBInsertQuery = "INSERT into {0} {1}";

            string insert = GenerateInsertString(defaultValues);

            command = connection.CreateCommand();
            command.CommandText = string.Format(DBInsertQuery, tableName, insert);
            command.ExecuteNonQuery();
            
            
            return GetDBObject(tableName, idName, actualId);
        }

        private int GetNextId(string tableName, string idName)
        {
            const string DBNextIdQuery = "SELECT {1} from {0} order by {1} desc limit 1";

            var command = connection.CreateCommand();
            command.CommandText = string.Format(DBNextIdQuery, tableName, idName);

            int id = (int)(decimal)command.ExecuteScalar();
            return id+1;
        }

        public Dictionary<string, object> GetDBObject(string tableName, string idName, int id)
        {
            const string DBObjectQuery = "SELECT * from {0} where {1}={2}";

            var command = connection.CreateCommand();
            command.CommandText = string.Format(DBObjectQuery, tableName, idName, id);

            var reader = command.ExecuteReader();
            if (!reader.HasRows)
                throw new ArgumentException("Object Not Found: " + tableName + ":" + idName + "=" + id);

            Dictionary<string, object> properties = new Dictionary<string, object>();
            if (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    properties[reader.GetName(i)] = reader.GetValue(i);
                }
            }

            return properties;
        }

        public void SaveDBObject(string tableName, string idName, Dictionary<string, object> properties)
        {
            const string DBSaveQueryFormat = "update {0} set {1} where {2}={3}";

            int id = (int)properties[idName];

            string update = GenerateUpdateString(idName, properties);

            var command = connection.CreateCommand();
            command.CommandText = string.Format(DBSaveQueryFormat, tableName, update, idName, id);
            
            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected != 1)
                System.Diagnostics.Debug.Assert(false);            
        }

        private static string GenerateInsertString(Dictionary<string, object> properties)
        {
            StringBuilder columnsBuilder = new StringBuilder();
            StringBuilder valuesBuilder = new StringBuilder();
            bool first = true;
            columnsBuilder.Append("(");
            valuesBuilder.Append("(");

            foreach (var kvp in properties)
            {
                if (!first)
                {
                    columnsBuilder.Append(", ");
                    valuesBuilder.Append(", ");
                }

                first = false;


                columnsBuilder.Append(kvp.Key);
                if (kvp.Value is string)
                    valuesBuilder.Append("\"" + kvp.Value + "\"");
                else
                    valuesBuilder.Append(kvp.Value.ToString());


            }
            valuesBuilder.Append(")");
            columnsBuilder.Append(") VALUES ");
            columnsBuilder.Append(valuesBuilder.ToString());
            return columnsBuilder.ToString();
        
        }
        private static string GenerateUpdateString(string idName, Dictionary<string, object> properties)
        {
            const string UpdateFragmentFormat = "{0}=\"{1}\"";
            StringBuilder updateBuilder = new StringBuilder();
            bool first = true;
            foreach (var kvp in properties)
            {
                if (kvp.Key.Equals(idName))
                    continue;


                if (!first)
                    updateBuilder.Append(", ");
                first = false;

                updateBuilder.AppendFormat(UpdateFragmentFormat, kvp.Key, kvp.Value);
            }
            return updateBuilder.ToString();
        }
    }

    
}
