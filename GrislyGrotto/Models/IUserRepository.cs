using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models
{
    public interface IUserRepository
    {
        UserInfo[] AllUsers();
        UserInfo GetUserByUsername(string username);
    }
}
