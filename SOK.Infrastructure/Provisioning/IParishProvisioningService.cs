using SOK.Domain.Entities.Central;

namespace SOK.Infrastructure.Provisioning
{
    /// <summary>
    /// Usługa do tworzenia indywidualnych baz danych dla parafii
    /// oraz zarządania ich stanem (istnienie, migracje).
    /// </summary>
    public interface IParishProvisioningService
    {
        /// <summary>
        /// Tworzy nową parafię w systemie. Przygotowuje bazę danych dla niej 
        /// (łącznie z użytkownikiem bazy i migracjami) i zapisuje zaszyfrowany 
        /// <c>ConnectionString</c> w modelu <see cref="ParishEntry"/>, 
        /// a model w centralnej bazie.
        /// </summary>
        /// <param name="parishUid">Publiczny unikalny identyfikator parafii (<c>UID</c>).</param>
        /// <param name="parishName">Nazwa parafii.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację,
        /// którego zawartością jest nowo utworzony wpis <see cref="ParishEntry"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="SystemException"></exception>
        Task<ParishEntry> CreateParishAsync(string parishUid, string parishName);

        /// <summary>
        /// Sprawdza, czy wszystkie parafialne bazy istnieją i mają aktualne migracje.
        /// Jeśli nie, to je tworzy i/lub wykonuje migracje.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację.
        /// </returns>
        Task EnsureAllParishDatabasesReadyAsync();
    }
}
