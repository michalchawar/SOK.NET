using Microsoft.Extensions.Configuration;
using SOK.Application.Services.Interface;
using System.Security.Cryptography;
using System.Text;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class CryptoService : ICryptoService
    {
        private readonly byte[] _key; // 256-bit
        private static readonly byte[] _salt = Encoding.UTF8.GetBytes("SOK.NET-Parish-Crypto-Salt-v1"); // Stała salt dla deterministycznego klucza

        public CryptoService(IConfiguration cfg)
        {
            // Użyj PBKDF2 do wygenerowania klucza z dowolnego hasła
            string password = cfg["Crypto:Key"]!;
            using var deriveBytes = new Rfc2898DeriveBytes(
                password, 
                _salt, 
                100000,
                HashAlgorithmName.SHA256
            );
            _key = deriveBytes.GetBytes(32); // Wygeneruj dokładnie 32 bajty (256 bitów) dla AES-256
        }

        /// <inheritdoc/>
        public string Encrypt(string plainMessage)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainText = System.Text.Encoding.UTF8.GetBytes(plainMessage);
            var cypherText = encryptor.TransformFinalBlock(plainText, 0, plainText.Length);

            // zapisz IV + CT (base64)
            return Convert.ToBase64String(aes.IV.Concat(cypherText).ToArray());
        }

        /// <inheritdoc/>
        public string Decrypt(string encryptedMessage)
        {
            var all = Convert.FromBase64String(encryptedMessage);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = all.Take(16).ToArray();

            using var decryptor = aes.CreateDecryptor();
            var cypherText = all.Skip(16).ToArray();
            var plainText = decryptor.TransformFinalBlock(cypherText, 0, cypherText.Length);

            return System.Text.Encoding.UTF8.GetString(plainText);
        }
    }

}
