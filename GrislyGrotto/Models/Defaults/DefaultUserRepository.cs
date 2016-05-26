using System.Linq;
using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models.Defaults
{
    public class DefaultUserRepository: IUserRepository
    {
        private UserInfo[] allUsers;

        public DefaultUserRepository()
        {
            allUsers = new UserInfo[3];

            allUsers[0] = new UserInfo("Christopher", "havoc", "shadow");
            allUsers[1] = new UserInfo("Peter", "pdc", "***REMOVED***");
            allUsers[2] = new UserInfo("Ben", "odin", "odin");
        }

        public UserInfo[] AllUsers()
        {
            return allUsers;
        }

        public UserInfo GetUserByUsername(string username)
        {
            var user = allUsers.Where(u => u.Username.Equals(username));
            if (user.Count() != 1)
                return null;
            else
                return user.Single();
        }
    }
}
