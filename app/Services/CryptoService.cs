using System.Security.Cryptography;

namespace app.Services
{
    /// <summary>
    /// Interfejs do szyfrowania i deszyfrowania danych.
    /// </summary>
    public interface ICryptoService 
    { 
        string Encrypt(string plainMessage); 
        string Decrypt(string encryptedMessage); 
    }

    /// <summary>
    /// Implementacja usługi szyfrowania i deszyfrowania danych.
    /// </summary>
    public class CryptoService : ICryptoService
    {
        private readonly byte[] _key; // 256-bit

        public CryptoService(IConfiguration cfg)
        {
            _key = Convert.FromBase64String(cfg["Crypto:Key"]);
        }

        public string Encrypt(string plain)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            
            using var enc = aes.CreateEncryptor();
            var pt = System.Text.Encoding.UTF8.GetBytes(plain);
            var ct = enc.TransformFinalBlock(pt, 0, pt.Length);
            
            // zapisz IV + CT (base64)
            return Convert.ToBase64String(aes.IV.Concat(ct).ToArray());
        }

        public string Decrypt(string enc)
        {
            var all = Convert.FromBase64String(enc);
            
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = all.Take(16).ToArray();
            
            using var dec = aes.CreateDecryptor();
            var ct = all.Skip(16).ToArray();
            var pt = dec.TransformFinalBlock(ct, 0, ct.Length);
            
            return System.Text.Encoding.UTF8.GetString(pt);
        }
    }

}
