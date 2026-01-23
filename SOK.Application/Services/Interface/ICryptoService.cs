namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do szyfrowania i deszyfrowania danych.
    /// </summary>
    public interface ICryptoService
    {
        /// <summary>
        /// Szyfruje podaną wiadomość jawną <paramref name="plainMessage"/> za pomocą algorytmu <c>AES</c>.
        /// Używa aktualnie skonfigurowanego klucza (wersja domyślna).
        /// </summary>
        /// <param name="plainMessage">Wiadomość (w kodowaniu <c>UTF-8</c>) do zaszyfrowania.</param>
        /// <returns>Szyfrogram w kodowaniu <c>Base64</c> z prefiksem wersji klucza.</returns>
        string Encrypt(string plainMessage);

        /// <summary>
        /// Szyfruje podaną wiadomość jawną <paramref name="plainMessage"/> za pomocą algorytmu <c>AES</c>
        /// używając klucza o określonej wersji.
        /// </summary>
        /// <param name="plainMessage">Wiadomość (w kodowaniu <c>UTF-8</c>) do zaszyfrowania.</param>
        /// <param name="keyVersion">Wersja klucza do użycia (1, 2, 3, etc.).</param>
        /// <returns>Szyfrogram w kodowaniu <c>Base64</c> z prefiksem wersji klucza.</returns>
        string Encrypt(string plainMessage, int keyVersion);

        /// <summary>
        /// Deszyfruje zaszyfrowaną przez metodę <see cref="Encrypt"/> wiadomość <paramref name="encryptedMessage"/>.
        /// Automatycznie wykrywa wersję klucza na podstawie prefiksu.
        /// </summary>
        /// <param name="encryptedMessage">Szyfrogram utworzony przez metodę <see cref="Encrypt"/> w kodowaniu <c>Base64</c>.</param>
        /// <returns>Tekst jawny wiadomości przed zaszyfrowaniem.</returns>
        string Decrypt(string encryptedMessage);

        /// <summary>
        /// Zwraca wersję aktualnie używanego klucza (domyślnego).
        /// </summary>
        /// <returns>Numer wersji aktualnego klucza.</returns>
        int GetCurrentKeyVersion();

        /// <summary>
        /// Re-enkryptuje wiadomość z jednego klucza na inny.
        /// Najpierw odszyfrowuje używając automatycznej detekcji klucza, 
        /// następnie szyfruje używając podanej <paramref name="targetKeyVersion"/>.
        /// </summary>
        /// <param name="encryptedMessage">Zaszyfrowana wiadomość do re-enkrypcji.</param>
        /// <param name="targetKeyVersion">Docelowa wersja klucza.</param>
        /// <returns>Wiadomość ponownie zaszyfrowana nowym kluczem.</returns>
        string Reencrypt(string encryptedMessage, int targetKeyVersion);
    }
}
