using System.Data;
using System.Data.SQLite;
using GrislyGrotto.Core;
using GrislyGrotto.Core.Entities;

namespace DAL.SQLite
{
    public class UserService : IUserService
    {
        private readonly DataAccess dataAccess;

        public UserService(ConnectionInfo connectionInfo)
        {
            dataAccess = new DataAccess(connectionInfo);
        }

        private static User UserFrom(DataRow row)
        {
            return new User
            {
                FullName = row["FullName"].ToString(),
                LoginName = row["LoginName"].ToString(),
                LoginPassword = row["LoginPassword"].ToString()
            };
        }

        public User GetUserByFullName(string fullName)
        {
            const string query = "SELECT * FROM Users WHERE FullName = @fullName";
            var results = dataAccess.RetrieveDataSet(query, new SQLiteParameter("@fullName", fullName));
            if (results.Tables.Count > 0 && results.Tables[0].Rows.Count > 0)
                return UserFrom(results.Tables[0].Rows[0]);
            return null;
        }

        public User Validate(string loginName, string loginPassword)
        {
            const string query = "SELECT * FROM Users WHERE LoginName = @loginName AND LoginPassword = @loginPassword";
            var results = dataAccess.RetrieveDataSet(query,
                new SQLiteParameter("@loginName", loginName),
                new SQLiteParameter("@loginPassword", loginPassword));
            if (results.Tables.Count > 0 && results.Tables[0].Rows.Count > 0)
                return UserFrom(results.Tables[0].Rows[0]);
            return null;
        }
    }
}