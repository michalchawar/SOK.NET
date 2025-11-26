using SOK.Application.Common.DTO;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do obsługi obiektów <see cref="ParishInfo"/>.
    /// </summary>
    public interface IParishInfoService
    {
        /// <summary>
        /// Pobiera informacje o wybranej parafii i binduje je
        /// na obiekt <see cref="ParishDto"/>.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="ParishDto"/>.
        /// </returns>
        Task<ParishDto> BindParishAsync();

        /// <summary>
        /// Pobiera wartość informacji o nazwie <paramref name="optionName"/>.
        /// </summary>
        /// <param name="optionName">Nazwa informacji.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest wartość informacji.
        /// </returns>
        /// <remarks>
        /// Jeśli informacja nie figuruje w bazie danych, funkcja zwraca <see cref="null"/>.
        /// </remarks>
        Task<string?> GetValueAsync(string optionName);

        /// <summary>
        /// Pobiera wartości informacji o nazwach z listy <paramref name="options"/>.
        /// </summary>
        /// <param name="options">Lista z nazwami pojedynczych informacji.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest słownik z kluczami i wartościami informacji.
        /// </returns>
        /// <remarks>
        /// Jeśli żadna wartość nie figuruje w bazie danych, funkcja zwraca pusty słownik.
        /// </remarks>
        Task<Dictionary<string, string>> GetValuesAsync(IEnumerable<string> options);

        /// <summary>
        /// Aktualizuje wartość informacji o nazwie <paramref name="optionName"/>.
        /// </summary>
        /// <param name="optionName">Nazwa informacji.</param>
        /// <param name="value">Nowa wartość informacji.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task UpdateValueAsync(string optionName, string value);

        /// <summary>
        /// Konwertuje wszystkie informacje na słownik, w którym kluczem
        /// są nazwy informacji, a wartościami ich wartości.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest słownik nazw informacji i odpowiadających im wartości
        /// konkretnego typu <see cref="Dictionary{string, string}"/>.
        /// </returns>
        Task<Dictionary<string, string>> GetDictionaryAsync();
    }
}