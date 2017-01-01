using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace GrislyGrotto
{
    public class Author
    {
        [Required, Key, Column("Username")]
        public string Username { get; set; }

        [NotMapped]
        public string PasswordHash
        {
            get { return Password?.Split(',')[0]; }
        }
        [NotMapped]
        public string PasswordSalt 
        { 
            get { return Password?.Split(',')[1]; }
        }

        [Required]
        public string Password { get; set; }
        
        public string DisplayName { get; set; }
        public string ImageUrl { get; set; }

        public static string GenerateSalt()
        {
            const int _saltLength = 384 / 8;
            using (var salter = RandomNumberGenerator.Create())
            {
                var buffer = new byte[_saltLength];
                salter.GetBytes(buffer);
                return Convert.ToBase64String(buffer);
            }
        }

        public static string GenerateHash(string password, string salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(salt))
                return null;

            var toHash = Encoding.UTF8.GetBytes(salt + password);
            using (var hasher = SHA384.Create())
            {
                var hash = hasher.ComputeHash(toHash);
                return Convert.ToBase64String(hash);
            }
        }
    }
}