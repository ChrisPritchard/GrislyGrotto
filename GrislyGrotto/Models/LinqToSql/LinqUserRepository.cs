using System.Collections.Generic;
using System.Linq;
using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models.LinqToSql
{
    public class LinqUserRepository : IUserRepository
    {
        private GrislyGrottoDBDataContext linqDataRepository;

        public LinqUserRepository(GrislyGrottoDBDataContext linqDataRepository)
        {
            this.linqDataRepository = linqDataRepository;
        }

        public UserInfo[] AllUsers()
        {
            var users = linqDataRepository.Users.ToList();

            var usersList = new List<UserInfo>();
            foreach (User user in users)
            {
                usersList.Add(new UserInfo(user.Fullname, user.Username, user.Password));
            }
            return usersList.ToArray();
        }

        public UserInfo GetUserByUsername(string username)
        {
            var user = linqDataRepository.Users.Where(u => u.Username.Equals(username)).Single();
            return new UserInfo(user.Fullname, user.Username, user.Password);
        }
    }
}
