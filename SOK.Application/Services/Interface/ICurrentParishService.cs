using SOK.Application.Common.DTO;
using SOK.Domain.Entities.Central;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Us³uga do ³adowania i przechowywania obecnie wybranej parafii.
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
        /// Ustawia wybran¹ parafiê na podstawie jej publicznego unikalnego identyfikatora <paramref name="parishUid"/>.
        /// </summary>
        /// <param name="parishUid">Publiczny unikalny identyfikator parafii (<c>UID</c>).</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy asynchroniczn¹ operacjê,
        /// którego zawartoœci¹ jest wartoœæ wskazuj¹ca, czy uda³o siê znaleŸæ
        /// i ustawiæ parafiê o okreœlonym <paramref name="parishUid"/>.
        /// </returns>
        Task<bool> SetParishAsync(string parishUid);

        /// <summary>
        /// Pobiera centraln¹ parafiê, odpowiadaj¹c¹ obecnej parafii, ustawionej w ¿¹daniu.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy asynchroniczn¹ operacjê,
        /// którego zawartoœci¹ jest obiekt <see cref="ParishEntry"/>, reprezentuj¹cy
        /// obecn¹ parafiê, ustawion¹ w obiekcie, lub <see cref="null"/> jeœli nie uda³o siê
        /// znaleŸæ parafii w bazie lub nie jest ona ustawiona.
        /// </returns>
        Task<ParishEntry?> GetCurrentParishAsync();
    }
}