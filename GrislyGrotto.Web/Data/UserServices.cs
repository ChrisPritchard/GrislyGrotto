using System.Data;
using System.Data.SQLite;
using GrislyGrotto.Web.Core;

namespace GrislyGrotto.Web.Data
{
    public static class UserServices
    {
        private static User UserFrom(DataRow row)
        {
            return new User
            {
                FullName = row["FullName"].ToString(),
                LoginName = row["LoginName"].ToString(),
                LoginPassword = row["LoginPassword"].ToString()
            };
        }

        public static User GetUserByFullName(string fullName)
        {
            const string query = "SELECT * FROM Users WHERE FullName LIKE @fullName";
            var results = DataAccess.Default.RetrieveDataSet(query, new SQLiteParameter("@fullName", fullName));
            if (results.Tables.Count > 0 && results.Tables[0].Rows.Count > 0)
                return UserFrom(results.Tables[0].Rows[0]);
            return null;
        }

        public static User Validate(string loginName, string loginPassword)
        {
            const string query = "SELECT * FROM Users WHERE LoginName = @loginName AND LoginPassword = @loginPassword";
            var results = DataAccess.Default.RetrieveDataSet(query,
                new SQLiteParameter("@loginName", loginName),
                new SQLiteParameter("@loginPassword", loginPassword));
            if (results.Tables.Count > 0 && results.Tables[0].Rows.Count > 0)
                return UserFrom(results.Tables[0].Rows[0]);
            return null;
        }
    }
}