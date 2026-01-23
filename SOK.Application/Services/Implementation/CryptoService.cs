using Microsoft.Extensions.Configuration;
using SOK.Application.Services.Interface;
using System.Security.Cryptography;
using System.Text;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class CryptoService : ICryptoService
    {
        private readonly Dictionary<int, byte[]> _keys = new(); // Mapa wersji klucza -> klucz
        private readonly int _currentKeyVersion; // Wersja aktualnie używanego klucza
        private static readonly byte[] _salt = Encoding.UTF8.GetBytes("SOK.NET-Parish-Crypto-Salt-v1");

        public CryptoService(IConfiguration cfg)
        {
            // Wczytaj wszystkie dostępne klucze z konfiguracji
            var cryptoSection = cfg.GetSection("Crypto:Keys");
            
            if (cryptoSection.Exists())
            {
                // Nowa konfiguracja z wieloma kluczami
                foreach (var keyConfig in cryptoSection.GetChildren())
                {
                    int version = int.Parse(keyConfig.Key);
                    string password = keyConfig.Value!;
                    _keys[version] = DeriveKey(password);
                }

                // Aktualny klucz to ten o najwyższej wersji
                _currentKeyVersion = _keys.Keys.Max();
            }
            else
            {
                // Fallback do starej konfiguracji (single key) - będzie miał wersję 1
                string password = cfg["Crypto:Key"]!;
                _keys[1] = DeriveKey(password);
                _currentKeyVersion = 1;
            }

            if (_keys.Count == 0)
            {
                throw new InvalidOperationException("No encryption keys configured. Please set Crypto:Keys in appsettings.json");
            }
        }

        /// <summary>
        /// Generuje klucz 256-bit z hasła przy użyciu PBKDF2
        /// </summary>
        private static byte[] DeriveKey(string password)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(
                password,
                _salt,
                100000, // liczba iteracji
                HashAlgorithmName.SHA256
            );
            return deriveBytes.GetBytes(32); // 32 bajty = 256 bitów
        }

        /// <inheritdoc/>
        public string Encrypt(string plainMessage)
        {
            return Encrypt(plainMessage, _currentKeyVersion);
        }

        /// <inheritdoc/>
        public string Encrypt(string plainMessage, int keyVersion)
        {
            if (!_keys.TryGetValue(keyVersion, out var key))
            {
                throw new ArgumentException($"Encryption key version {keyVersion} not found in configuration.", nameof(keyVersion));
            }

            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainText = Encoding.UTF8.GetBytes(plainMessage);
            var cypherText = encryptor.TransformFinalBlock(plainText, 0, plainText.Length);

            // Zapisz IV + Cipher Text
            var encrypted = Convert.ToBase64String(aes.IV.Concat(cypherText).ToArray());

            // Dodaj prefix wersji: v{KeyVersion}:{EncryptedData}
            return $"v{keyVersion}:{encrypted}";
        }

        /// <inheritdoc/>
        public string Decrypt(string encryptedMessage)
        {
            // Wykryj wersję klucza z prefiksu
            int keyVersion = 1; // domyślna wersja dla starych danych bez prefiksu
            string base64Data = encryptedMessage;

            if (encryptedMessage.StartsWith("v") && encryptedMessage.Contains(':'))
            {
                var parts = encryptedMessage.Split(':', 2);
                if (parts.Length == 2 && int.TryParse(parts[0].Substring(1), out var version))
                {
                    keyVersion = version;
                    base64Data = parts[1];
                }
            }

            if (!_keys.TryGetValue(keyVersion, out var key))
            {
                throw new InvalidOperationException($"Decryption key version {keyVersion} not found. Cannot decrypt data encrypted with key v{keyVersion}.");
            }

            var all = Convert.FromBase64String(base64Data);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = all.Take(16).ToArray();

            using var decryptor = aes.CreateDecryptor();
            var cypherText = all.Skip(16).ToArray();
            var plainText = decryptor.TransformFinalBlock(cypherText, 0, cypherText.Length);

            return Encoding.UTF8.GetString(plainText);
        }

        /// <inheritdoc/>
        public int GetCurrentKeyVersion()
        {
            return _currentKeyVersion;
        }

        /// <inheritdoc/>
        public string Reencrypt(string encryptedMessage, int targetKeyVersion)
        {
            // Odszyfruj starym kluczem
            string plainText = Decrypt(encryptedMessage);
            
            // Zaszyfruj nowym kluczem
            return Encrypt(plainText, targetKeyVersion);
        }
    }

}
