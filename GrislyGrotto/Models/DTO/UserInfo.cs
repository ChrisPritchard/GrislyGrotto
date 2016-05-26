namespace GrislyGrotto.Models.DTO
{
    public class UserInfo
    {
        public string Fullname { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public UserInfo(string fullname, string username)
        {
            Fullname = fullname;
            Username = username;
        }

        public UserInfo(string fullname, string username, string password)
        {
            Fullname = fullname;
            Username = username;
            Password = password;
        }
    }
}
