using System.Security.Cryptography;
using System.Text;

namespace S3.Gateway.Security
{
    public static class PartnerCredentialSecret
    {
        public static string Generate(int byteLength = 32)
        {
            return Base64UrlEncode(RandomNumberGenerator.GetBytes(byteLength));
        }

        public static string Hash(string secret)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));
        }

        public static bool Verify(string secret, string expectedHash)
        {
            try
            {
                var actual = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
                var expected = Convert.FromBase64String(expectedHash);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string Base64UrlEncode(byte[] value)
        {
            return Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
