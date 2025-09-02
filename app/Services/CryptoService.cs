using System.Security.Cryptography;

namespace app.Services
{
    /// <summary>
    /// Implementacja usługi szyfrowania i deszyfrowania danych.
    /// </summary>
    public class CryptoService : ICryptoService
    {
        private readonly byte[] _key; // 256-bit

        public CryptoService(IConfiguration cfg)
        {
            _key = Convert.FromBase64String(cfg["Crypto:Key"]!);
        }

        public string Encrypt(string plainMessage)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            
            using var encryptor = aes.CreateEncryptor();
            var plainText  = System.Text.Encoding.UTF8.GetBytes(plainMessage);
            var cypherText = encryptor.TransformFinalBlock(plainText, 0, plainText.Length);
            
            // zapisz IV + CT (base64)
            return Convert.ToBase64String(aes.IV.Concat(cypherText).ToArray());
        }

        public string Decrypt(string encryptedMessage)
        {
            var all = Convert.FromBase64String(encryptedMessage);
            
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = all.Take(16).ToArray();
            
            using var decryptor = aes.CreateDecryptor();
            var cypherText = all.Skip(16).ToArray();
            var plainText  = decryptor.TransformFinalBlock(cypherText, 0, cypherText.Length);
            
            return System.Text.Encoding.UTF8.GetString(plainText);
        }
    }

}
