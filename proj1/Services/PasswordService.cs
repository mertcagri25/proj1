using System.Security.Cryptography;
using System.Text;

namespace proj1.Services
{
    public class PasswordService
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var bytes = pbkdf2.GetBytes(HashSize);
            return $"PBKDF2|{Iterations}|{Convert.ToBase64String(salt)}|{Convert.ToBase64String(bytes)}";
        }

        public bool VerifyPassword(string hash, string password)
        {
            if (string.IsNullOrEmpty(hash)) return false;

            if (hash.StartsWith("PBKDF2|"))
            {
                return VerifyPBKDF2(hash, password);
            }
            else
            {
                return VerifyLegacySha256(hash, password);
            }
        }

        public bool IsRehashNeeded(string hash)
        {
            return !hash.StartsWith("PBKDF2|");
        }

        private bool VerifyPBKDF2(string stored, string password)
        {
            var parts = stored.Split('|');
            if (parts.Length != 4) return false;
            if (!int.TryParse(parts[1], out var iterations)) return false;
            
            byte[] salt;
            byte[] storedHash;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                storedHash = Convert.FromBase64String(parts[3]);
            }
            catch { return false; }

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var computed = pbkdf2.GetBytes(storedHash.Length);
            return CryptographicOperations.FixedTimeEquals(computed, storedHash);
        }

        private bool VerifyLegacySha256(string stored, string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var computed = Convert.ToBase64String(hashedBytes);
            return stored == computed;
        }
    }
}
