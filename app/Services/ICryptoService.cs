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
}
