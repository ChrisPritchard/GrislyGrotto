using System;
using System.Data;
using System.Data.SQLite;
using System.Web;

namespace GrislyGrotto.Data
{
    internal class DataAccess
    {
        public static DataAccess Default
        {
            get
            {
                return new DataAccess
                {
                    ConnectionString = string.Format("Data Source={0};Pooling=true;FailIfMissing=true",
                        HttpContext.Current.Server.MapPath(@"\App_Data\GrislyGrotto.db3"))
                };
            }
        }

        public string ConnectionString { get; set; }

        public DataSet RetrieveDataSet(string query, params SQLiteParameter[] parameters)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new ArgumentNullException(ConnectionString);

            var dataSet = new DataSet("Results");

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                var command = new SQLiteCommand(query, connection);
                command.Parameters.AddRange(parameters);
                var adapter = new SQLiteDataAdapter(command);

                connection.Open();
                adapter.Fill(dataSet);
                connection.Close();
            }

            return dataSet;
        }

        public void ExecuteNonQuery(string query, params SQLiteParameter[] parameters)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new ArgumentNullException(ConnectionString);

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                var command = new SQLiteCommand(query, connection);
                command.Parameters.AddRange(parameters);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
