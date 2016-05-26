using System.Collections.Generic;
using System.Linq;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.Website.Models.Defaults
{
    public class DefaultUserRepository: IUserRepository
    {
        private User[] allUsers;

        public DefaultUserRepository()
        {
            allUsers = new User[3];

            allUsers[0] = new User("Christopher", "havoc", "shadow");
            allUsers[1] = new User("Peter", "pdc", "***REMOVED***");
            allUsers[2] = new User("Ben", "odin", "odin");
        }

        public IEnumerable<User> AllUsers()
        {
            return allUsers;
        }

        public User GetUserByUsername(string username)
        {
            var user = allUsers.Where(u => u.Username.Equals(username));
            if (user.Count() != 1)
                return null;
            else
                return user.Single();
        }
    }
}
