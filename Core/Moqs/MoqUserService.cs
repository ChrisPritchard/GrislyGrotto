using System.Linq;
using GrislyGrotto.Core.Entities;

namespace GrislyGrotto.Core.Moqs
{
    public class MoqUserService : IUserService
    {
        private readonly User[] moqUsers;

        public MoqUserService()
        {
            moqUsers = new []
               {
                   new User { FullName = "Christopher", LoginName = "havoc", LoginPassword = "test"},
                   new User { FullName = "Peter", LoginName = "pdc", LoginPassword = "test" },
                   new User { FullName = "Ben", LoginName = "odin", LoginPassword = "test" }
               };
        }

        public User GetUserByFullName(string fullName)
        {
            return moqUsers.SingleOrDefault(u => u.FullName.Equals(fullName));
        }

        public User Validate(string loginName, string loginPassword)
        {
            return moqUsers.SingleOrDefault(u => u.LoginName.Equals(loginName) && u.LoginPassword.Equals(loginPassword));
        }

        public User GetUserByLoginName(string loginName)
        {
            return moqUsers.SingleOrDefault(u => u.LoginName.Equals(loginName));
        }
    }
}