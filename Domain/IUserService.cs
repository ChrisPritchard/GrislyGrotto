using GrislyGrotto.Core.Entities;

namespace GrislyGrotto.Core
{
    public interface IUserService
    {
        User GetUserByFullName(string fullName);
        User Validate(string loginName, string loginPassword);
        User GetUserByLoginName(string loginName);
    }
}