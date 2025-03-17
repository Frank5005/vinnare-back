using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Shared.Configuration;

namespace Services.Utils
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly string _pepper;

        public PasswordHasher(IOptions<SecuritySettings> options)
        {
            _pepper = options.Value.PasswordPepper ?? throw new ArgumentNullException("Password pepper is not configured.");
        }

        public string HashPassword(string password)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            string salt = Convert.ToBase64String(saltBytes);

            string saltedPassword = password + salt + _pepper;

            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

            return $"{salt}.{Convert.ToBase64String(hashBytes)}"; // Store as "salt.hash"
        }

        public bool VerifyPassword(string password, string storedValue)
        {
            var parts = storedValue.Split('.');
            if (parts.Length != 2) return false;

            string salt = parts[0];
            string storedHash = parts[1];

            string saltedPassword = password + salt + _pepper;

            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

            string computedHash = Convert.ToBase64String(hashBytes);
            return storedHash == computedHash;
        }
    }
}
