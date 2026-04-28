using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace S3.Gateway.Integrations.eKYC.Napas
{
    public static class RSASignatureService
    {
        #region Old
        //public static string Sign(string data, string p12Path, string password)
        //{
        //    var cert = new X509Certificate2(
        //        p12Path,
        //        password,
        //        X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet
        //    );

        //    using RSA rsa = cert.GetRSAPrivateKey();

        //    byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        //    byte[] signature = rsa.SignData(
        //        dataBytes,
        //        HashAlgorithmName.SHA256,
        //        RSASignaturePadding.Pkcs1
        //    );

        //    return Convert.ToBase64String(signature);
        //}

        //public static bool Verify(string data, string signatureBase64, string publicKeyPath)
        //{
        //    string pem = File.ReadAllText(publicKeyPath);

        //    using RSA rsa = RSA.Create();
        //    rsa.ImportFromPem(pem);

        //    byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        //    byte[] signature = Convert.FromBase64String(signatureBase64);

        //    return rsa.VerifyData(
        //        dataBytes,
        //        signature,
        //        HashAlgorithmName.SHA256,
        //        RSASignaturePadding.Pkcs1
        //    );
        //}
        #endregion

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
            string publicKeyPem = File.ReadAllText(publicKeyPath);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem.ToCharArray());

            byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] signature = Convert.FromBase64String(signatureBase64);

            return rsa.VerifyData(
                data,
                signature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
    }
}
