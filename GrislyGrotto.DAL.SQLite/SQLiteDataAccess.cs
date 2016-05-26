using System.Data;
using System.Data.SQLite;

namespace GrislyGrotto.DAL.SQLite
{
    public class SQLiteDataAccess
    {
        readonly string connectionString;

        public SQLiteDataAccess(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void ExecuteNonQuery(string query, params SQLiteParameter[] parameters)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                var command = new SQLiteCommand(query, connection);
                command.Parameters.AddRange(parameters);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public int ExecuteScalar(string query, params SQLiteParameter[] parameters)
        {
            var result = 0;
            using (var connection = new SQLiteConnection(connectionString))
            {
                var command = new SQLiteCommand(query, connection);
                command.Parameters.AddRange(parameters);

                connection.Open();
                result = (int)command.ExecuteScalar();
                connection.Close();
            }
            return result;
        }

        public DataSet RetrieveDataSet(string query, params SQLiteParameter[] parameters)
        {
            var dataSet = new DataSet();

            using (var connection = new SQLiteConnection(connectionString))
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
    }
}
