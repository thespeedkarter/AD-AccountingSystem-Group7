using System.Security.Cryptography;
using System.Text;

namespace AccountingSystem.Security
{
    public static class SecurityAnswerHasher
    {
        public static string Hash(string input)
        {
            input ??= "";
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input.Trim().ToLowerInvariant()));
            return Convert.ToHexString(bytes);
        }

        public static bool Verify(string input, string expectedHash)
        {
            if (string.IsNullOrWhiteSpace(expectedHash)) return false;
            var actual = Hash(input);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(actual),
                Encoding.UTF8.GetBytes(expectedHash.Trim()));
        }
    }
}