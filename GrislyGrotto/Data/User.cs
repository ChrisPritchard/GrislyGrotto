using System;
using System.Security.Cryptography;
using System.Text;

namespace GrislyGrotto.Data
{
    public class User
    {
        public int ID { get; set; }

        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }

        public static string GetSalt()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GetSaltedPassword(string plainPassword, string salt)
        {
            var hashWithSalt = plainPassword + salt;
            var saltedHashBytes = Encoding.UTF8.GetBytes(hashWithSalt);
            var algorithm = new SHA256Managed();
            var hash = algorithm.ComputeHash(saltedHashBytes);
            
            return Convert.ToBase64String(hash);
        }

        public bool IsPasswordAMatch(string plainPassword)
        {
            return GetSaltedPassword(plainPassword, Salt).Equals(Password);
        }
    }
}