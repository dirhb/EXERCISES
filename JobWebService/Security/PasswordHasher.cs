using System.Security.Cryptography;

namespace JobWebService
{
    // Salted PBKDF2 (SHA-256) password hashing using only the framework — no
    // extra NuGet packages. A stored value looks like "iterations.salt.hash"
    // (all base64), which fits comfortably in the [User].Password text column.
    public static class PasswordHasher
    {
        private const int SaltSize = 16;     // 128-bit salt
        private const int KeySize = 32;      // 256-bit derived key
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
        private const char Separator = '.';

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
            return string.Join(Separator, Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(key));
        }

        public static bool Verify(string password, string? stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;

            string[] parts = stored.Split(Separator, 3);
            if (parts.Length != 3) return false;            // not in our format (e.g. legacy plaintext)
            if (!int.TryParse(parts[0], out int iterations)) return false;

            byte[] salt, key;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                key = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] attempt = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, key.Length);
            return CryptographicOperations.FixedTimeEquals(attempt, key);
        }

        // True if the stored value is one of our hashes (vs. a legacy plaintext password).
        public static bool LooksHashed(string? stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;
            string[] parts = stored.Split(Separator, 3);
            return parts.Length == 3 && int.TryParse(parts[0], out _);
        }
    }
}
