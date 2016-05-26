using System.Runtime.Serialization;

namespace GrislyGrotto.Models
{
    public class User
    {
        public int ID { get; set; }
        
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }

        public string DisplayName { get; set; }
        public string ImageUrl { get; set; }
    }
}
