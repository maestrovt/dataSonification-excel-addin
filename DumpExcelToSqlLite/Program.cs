using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data;
using System.Data.SQLite;

namespace DumpExcelToSqlLite
{
    class Program
    {
        static void Main(string[] args)
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = args[1];

            SQLiteConnection sqlLiteConnection = new SQLiteConnection(builder.ConnectionString);
            sqlLiteConnection.Open();

           
            OdbcConnection connection = new OdbcConnection( @"Driver={Microsoft Excel Driver (*.xls, *.xlsx, *.xlsm, *.xlsb)};dbq=C:\Users\ben\Documents\Development\CoreDeploy\dataSonification.xls;defaultdir=C:\Users\ben\AppData\Local\temp;driverid=1046;maxbuffersize=2048;pagetimeout=5");//  @"Driver={Microsoft Excel Driver (*.xls, *.xlsx, *.xlsm, *.xlsb)};dbq=" + args[0] + ";defaultdir=" + System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\temp;driverid=1046;maxbuffersize=2048;pagetimeout=5");
            connection.Open();
            var schema = connection.GetSchema("Tables");
            List<string> tables = new List<string>();
            foreach (DataRow row in schema.Rows)
            {
                if (row.ItemArray[3].Equals("TABLE"))
                {
                    tables.Add((string)row.ItemArray[2]);
                }
            }

            foreach (var table in tables)
            {
                DumpTable(table, connection, sqlLiteConnection);
            }


            connection.Close();



        }

        private static void DumpTable(string table, OdbcConnection connection, SQLiteConnection sqlLiteConnection)
        {
            Console.WriteLine("Table: " + table);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * from " + table;

                


                var reader = command.ExecuteReader();

                var schemaTable = reader.GetSchemaTable();

                using (var sqlliteCommand = sqlLiteConnection.CreateCommand())
                {
                    StringBuilder createTableBuilder = new StringBuilder();
                    StringBuilder columnsBuilder = new StringBuilder();

                    columnsBuilder.Append("(");
                    createTableBuilder.Append("create table " + table + "(");
                    bool first = true;

                    foreach (DataRow dr in schemaTable.Rows)
                    {
                        if (!first)
                        {
                            createTableBuilder.Append(", ");
                            columnsBuilder.Append(", ");
                        }

                        first = false;

                        string dataType = (Type)(dr["DataType"]) == typeof(System.String) ? "varchar(100)" : "numeric";
                        createTableBuilder.AppendFormat("{0} {1}", dr["ColumnName"], dataType);
                        columnsBuilder.Append(dr["ColumnName"]);
                        
                    }
                    createTableBuilder.Append(")");
                    columnsBuilder.Append(")");
                    Console.WriteLine(createTableBuilder.ToString());
                    sqlliteCommand.CommandText = createTableBuilder.ToString();

                    sqlliteCommand.ExecuteNonQuery();
                    string columns = columnsBuilder.ToString();
                    while (reader.Read())
                    {

                        StringBuilder insertRowBuilder = new StringBuilder();
                        insertRowBuilder.Append("insert into " + table + " " + columns + " values (");
                        first = true;
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (!first)
                                insertRowBuilder.Append(", ");
                            first = false;
                            insertRowBuilder.Append("\"" + reader.GetValue(i) + "\"");
                        }
                        insertRowBuilder.Append(")");
                        Console.WriteLine(insertRowBuilder.ToString());
                        sqlliteCommand.CommandText = insertRowBuilder.ToString();
                        sqlliteCommand.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
