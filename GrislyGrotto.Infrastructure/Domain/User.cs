
namespace GrislyGrotto.Infrastructure.Domain
{
    public class User
    {
        public string Fullname { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public User(string fullname, string username, string password)
        {
            Fullname = fullname;
            Username = username;
            Password = password;
        }
    }
}
