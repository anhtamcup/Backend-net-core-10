using System.Security.Cryptography;
using System.Text;

namespace DD.Gateway.Security;

public static class PartnerCredentialSecret
{
    public static string Generate(int byteLength = 32) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteLength))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string Hash(string secret) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));

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
}
