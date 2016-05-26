using System.Collections.Generic;

namespace GrislyGrotto.Framework.Data
{
    public interface IUserData
    {
        IEnumerable<string> AllUsernames();
        bool ValidateCredentials(string username, string password);
        string FullNameOf(string username);
    }
}