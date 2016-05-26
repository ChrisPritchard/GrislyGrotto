using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using GrislyGrotto.Infrastructure;
using Domain = GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.DAL.SQLServer
{
    public class SQLServerUserRepository : IUserRepository
    {
        private GrislyGrottoDBDataContext linqDataRepository;

        public SQLServerUserRepository(ConnectionStringSettingsCollection connectionStrings)
        {
            this.linqDataRepository = new GrislyGrottoDBDataContext(connectionStrings);
        }

        public IEnumerable<Domain.User> AllUsers()
        {
            var users = linqDataRepository.Users.ToList();

            foreach (User user in users)
            {
                yield return new Domain.User(user.Fullname, user.Username, user.Password);
            }
        }

        public Domain.User GetUserByUsername(string username)
        {
            var user = linqDataRepository.Users.Where(u => u.Username.Equals(username)).Single();
            return new Domain.User(user.Fullname, user.Username, user.Password);
        }
    }
}
