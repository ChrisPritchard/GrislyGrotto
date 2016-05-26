using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace GrislyGrotto.Framework.Data.Implementations
{
    public class SqlUserData : IUserData
    {
        public IEnumerable<string> AllUsernames()
        {
            const string query = "SELECT FullName FROM Users";
            var results = DataAccess.Default.RetrieveDataSet(query);
            return results.Tables[0].Rows.Cast<DataRow>().Select(r => r["FullName"].ToString());
        }

        public bool ValidateCredentials(string username, string password)
        {
            const string query = "SELECT * FROM Users WHERE LoginName = @loginName AND LoginPassword = @loginPassword";
            var results = DataAccess.Default.RetrieveDataSet(query,
                new SQLiteParameter("@loginName", username),
                new SQLiteParameter("@loginPassword", password));
            return results.Tables.Count > 0 && results.Tables[0].Rows.Count > 0;
        }

        public string FullNameOf(string username)
        {
            const string query = "SELECT FullName FROM Users WHERE LoginName = @loginName";
            var results = DataAccess.Default.RetrieveDataSet(query,
                new SQLiteParameter("@loginName", username));
            return results.Tables.Count > 0 && results.Tables[0].Rows.Count > 0
                ? results.Tables[0].Rows.Cast<DataRow>().Select(r => r["FullName"].ToString()).First()
                : null;
        }
    }
}