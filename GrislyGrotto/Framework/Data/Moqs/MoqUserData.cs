using System.Collections.Generic;

namespace GrislyGrotto.Framework.Data.Moqs
{
    public class MoqUserData : IUserData
    {
        public IEnumerable<string> AllUsernames()
        {
            return new [] { "Christopher", "Peter" };
        }

        public bool ValidateCredentials(string username, string password)
        {
            return
                (username.Equals("Christopher") && password.Equals("test1"))
                || (username.Equals("Peter") && password.Equals("test2"));
        }

        public string FullNameOf(string username)
        {
            return username;
        }
    }
}