using System;
using System.Security.Cryptography;
using System.Text;

namespace Migration.FromBlobToSql
{
    class Salting
    {
        public static string GetSaltedPassword(string plainPassword, string salt)
        {
            var hashWithSalt = plainPassword + salt;
            var saltedHashBytes = Encoding.UTF8.GetBytes(hashWithSalt);
            var algorithm = new SHA256Managed();
            var hash = algorithm.ComputeHash(saltedHashBytes);

            return Convert.ToBase64String(hash);
        }
    }
}
