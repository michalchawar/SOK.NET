using System.Security.Cryptography;

namespace app.Services
{
    /// <summary>
    /// Interfejs do szyfrowania i deszyfrowania danych.
    /// </summary>
    public interface ICryptoService 
    {
        /// <summary>
        /// Szyfruje podaną wiadomość jawną za pomocą algorytmu AES.
        /// </summary>
        /// <param name="plainMessage">Wiadomość (w kodowaniu UTF-8) do zaszyfrowania</param>
        /// <returns>Szyfrogram w kodowaniu Base64</returns>
        string Encrypt(string plainMessage); 
        
        /// <summary>
        /// Deszyfruje zaszyfrowaną przez metodę Encrypt wiadomość.
        /// </summary>
        /// <param name="encryptedMessage">Szyfrogram utworzony przez metodę Encrypt w kodowaniu Base64</param>
        /// <returns>Tekst jawny wiadomości przed zaszyfrowaniem</returns>
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
            _key = Convert.FromBase64String(cfg["Crypto:Key"]!);
        }

        public string Encrypt(string plain)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            
            using var encryptor = aes.CreateEncryptor();
            var plainText  = System.Text.Encoding.UTF8.GetBytes(plain);
            var cypherText = encryptor.TransformFinalBlock(plainText, 0, plainText.Length);
            
            // zapisz IV + CT (base64)
            return Convert.ToBase64String(aes.IV.Concat(cypherText).ToArray());
        }

        public string Decrypt(string encrypted)
        {
            var all = Convert.FromBase64String(encrypted);
            
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
