using System.Collections.Generic;
using GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.Infrastructure
{
    public interface IUserRepository
    {
        IEnumerable<User> AllUsers();
        User GetUserByUsername(string username);
    }
}
