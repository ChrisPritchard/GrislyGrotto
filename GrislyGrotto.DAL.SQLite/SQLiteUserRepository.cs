using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.DAL.SQLite
{
    public class SQLiteUserRepository : IUserRepository
    {
        readonly SQLiteDataAccess dataAccess;

        public SQLiteUserRepository(SQLiteDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        private IEnumerable<User> MapDatasetToUsers(DataSet dataSet)
        {
            if (dataSet.Tables.Count == 0)
                yield break;

            var table = dataSet.Tables[0];
            if(table.Rows.Count == 0)
                yield break;

            foreach (DataRow row in table.Rows)
                yield return new User(
                    row.Field<string>("Author"), 
                    row.Field<string>("Username"), 
                    row.Field<string>("Password"));
        }

        public IEnumerable<User> AllUsers()
        {
            return MapDatasetToUsers(dataAccess.RetrieveDataSet("select * from Authors"));
        }

        public User GetUserByUsername(string username)
        {
            var results = MapDatasetToUsers(dataAccess.RetrieveDataSet("select * from Authors where Username = @username",
                new SQLiteParameter("@username", username)));

            return results.Count() > 0 ? results.First() : null;
        }
    }
}
