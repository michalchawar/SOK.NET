using SOK.Application.Common.DTO;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do obsługi obiektów ParishInfo.
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
        /// Jeśli informacja nie figuruje w bazie danych, funkcja zwraca <see cref="string.Empty"/>.
        /// </remarks>
        Task<string> GetValueAsync(string optionName);

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
        /// którego zawartością jest słownik nazw informacji i odpowiadających im wartości.
        /// </returns>
        Task<Dictionary<string, string>> GetDictionary();
    }
}