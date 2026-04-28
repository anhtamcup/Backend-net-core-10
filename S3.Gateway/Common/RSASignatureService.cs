using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace S3.Gateway.Common
{
    public static class RSASignatureService
    {
        public static string SignMessage(string message, string privateKeyPath)
        {
            // đọc private key từ file
            string privateKeyPem = File.ReadAllText(privateKeyPath);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem.ToCharArray());

            byte[] data = Encoding.UTF8.GetBytes(message);

            byte[] signature = rsa.SignData(
                data,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signature);
        }

        public static bool VerifySignature(string message, string signatureBase64, string publicKeyPath)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty");

            if (string.IsNullOrWhiteSpace(signatureBase64))
                throw new ArgumentException("Signature cannot be empty");

            if (!System.IO.File.Exists(publicKeyPath))
                throw new ArgumentException("Public key file not found");

            // Load certificate
            var cert = new X509Certificate2(publicKeyPath);

            using RSA rsa = cert.GetRSAPublicKey();

            byte[] dataBytes = Encoding.UTF8.GetBytes(message);
            byte[] signatureBytes = Convert.FromBase64String(signatureBase64);

            return rsa.VerifyData(
                dataBytes,
                signatureBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
    }
}
