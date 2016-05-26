using System.Data;
using System.Data.SQLite;

namespace DAL.SQLite
{
    public class DataAccess
    {
        private readonly ConnectionInfo connectionInfo;

        public DataAccess(ConnectionInfo targetConnectionInfo)
        {
            connectionInfo = targetConnectionInfo;
        }

        public DataSet RetrieveDataSet(string query, params SQLiteParameter[] parameters)
        {
            var dataSet = new DataSet("Results");

            using (var connection = new SQLiteConnection(connectionInfo.ConnectionString))
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
            using (var connection = new SQLiteConnection(connectionInfo.ConnectionString))
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
