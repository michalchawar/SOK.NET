namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do szyfrowania i deszyfrowania danych.
    /// </summary>
    public interface ICryptoService
    {
        /// <summary>
        /// Szyfruje podaną wiadomość jawną <paramref name="plainMessage"/> za pomocą algorytmu <c>AES</c>.
        /// </summary>
        /// <param name="plainMessage">Wiadomość (w kodowaniu <c>UTF-8</c>) do zaszyfrowania.</param>
        /// <returns>Szyfrogram w kodowaniu <c>Base64</c>.</returns>
        string Encrypt(string plainMessage);

        /// <summary>
        /// Deszyfruje zaszyfrowaną przez metodę <see cref="Encrypt"/> wiadomość <paramref name="encryptedMessage"/>.
        /// </summary>
        /// <param name="encryptedMessage">Szyfrogram utworzony przez metodę <see cref="Encrypt"/> w kodowaniu <c>Base64</c>.</param>
        /// <returns>Tekst jawny wiadomości przed zaszyfrowaniem.</returns>
        string Decrypt(string encryptedMessage);
    }
}
