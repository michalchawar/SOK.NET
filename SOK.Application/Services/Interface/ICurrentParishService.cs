using SOK.Domain.Entities.Central;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do ładowania i przechowywania obecnie wybranej parafii.
    /// </summary>
    public interface ICurrentParishService
    {
        /// <summary>
        /// Publiczny unikalny identyfikator obecnie wybranej parafii.
        /// </summary>
        public string? ParishUid { get; }

        /// <summary>
        /// <c>ConnectionString</c> wybranej parafii w postaci jawnej.
        /// </summary>
        public string? ConnectionString { get; }

        /// <summary>
        /// Ustawia wybraną parafię na podstawie jej publicznego unikalnego identyfikatora <paramref name="parishUid"/>.
        /// </summary>
        /// <param name="parishUid">Publiczny unikalny identyfikator parafii (<c>UID</c>).</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację,
        /// którego zawartością jest wartość wskazująca, czy udało się znaleźć
        /// i ustawić parafię o określonym <paramref name="parishUid"/>.
        /// </returns>
        Task<bool> SetParishAsync(string parishUid);

        /// <summary>
        /// Pobiera centralną parafię, odpowiadającą obecnej parafii, ustawionej w żądaniu.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację,
        /// którego zawartością jest obiekt <see cref="ParishEntry"/>, reprezentujący
        /// obecną parafię, ustawioną w obiekcie, lub <see cref="null"/> jeśli nie udało się
        /// znaleźć parafii w bazie lub nie jest ona ustawiona.
        /// </returns>
        Task<ParishEntry?> GetCurrentParishAsync();

        /// <summary>
        /// Sprawdza, czy parafia została już ustawiona.
        /// </summary>
        /// <returns>
        /// <c>true</c>, jeśli parafia jest ustawiona; w przeciwnym razie <c>false</c>.
        /// </returns>
        bool IsParishSet();
    }
}