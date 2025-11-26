using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium informacji o parafii.
    /// </summary>
    public interface IParishInfoRepository : IUpdatableRepository<ParishInfo>
    {
        Task<string?> GetValueAsync(string name);
        
        Task SetValueAsync(string name, string value);

        Task ClearValueAsync(string name);

        Task<Dictionary<string, string>> ToDictionaryAsync();

        /// <summary>
        /// Pobiera wartości informacji o nazwach z listy <paramref name="options"/>.
        /// </summary>
        /// <param name="options">Nazwy pojedynczych informacji do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest słownik z kluczami i wartościami informacji.
        /// </returns>
        Task<Dictionary<string, string>> GetValuesAsDictionaryAsync(IEnumerable<string> options);
    }
}