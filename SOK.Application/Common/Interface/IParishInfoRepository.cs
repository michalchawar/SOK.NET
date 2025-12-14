using SOK.Domain.Entities.Parish;
using SOK.Domain.Interfaces;

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

        /// <summary>
        /// Zapisuje wartość metadanych dla encji.
        /// </summary>
        /// <typeparam name="T">Typ encji implementującej interfejs <see cref="IEntityMetadata"/>.</typeparam>
        /// <param name="entity">Encja, dla której zapisywane są metadane.</param>
        /// <param name="propertyName">Nazwa właściwości metadanych.</param>
        /// <param name="value">Wartość metadanych do zapisania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący operację asynchroniczną.
        /// </returns>
        Task SetMetadataAsync<T>(T entity, string propertyName, string value) 
            where T : IEntityMetadata;

        /// <summary>
        /// Pobiera wartość metadanych dla encji.
        /// </summary>
        /// <typeparam name="T">Typ encji implementującej interfejs <see cref="IEntityMetadata"/>.</typeparam>
        /// <param name="entity">Encja, dla której pobierane są metadane.</param>
        /// <param name="propertyName">Nazwa właściwości metadanych.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący operację asynchroniczną, którego wartością jest
        /// wartość metadanej jako <see cref="string"/> lub <see cref="null"/>, jeśli metadane nie istnieją.
        /// </returns>
        Task<string?> GetMetadataAsync<T>(T entity, string propertyName) 
            where T : IEntityMetadata;

        /// <summary>
        /// Usuwa metadane dla określonej encji i właściwości.
        /// </summary>
        /// <typeparam name="T">Typ encji implementującej interfejs <see cref="IEntityMetadata"/>.</typeparam>
        /// <param name="entity">Encja, dla której usuwane są metadane.</param>
        /// <param name="propertyName">Nazwa właściwości metadanych.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący operację asynchroniczną.
        /// </returns>
        Task DeleteMetadataAsync<T>(T entity, string propertyName) 
            where T : IEntityMetadata;

        /// <summary>
        /// Pobiera wszystkie metadane dla danej encji.
        /// </summary>
        /// <typeparam name="T">Typ encji implementującej interfejs <see cref="IEntityMetadata"/>.</typeparam>
        /// <param name="entity">Encja, dla której pobierane są metadane.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący operację asynchroniczną, którego wartością jest słownik metadanych.
        /// </returns>
        Task<Dictionary<string, string>> GetAllMetadataAsync<T>(T entity) 
            where T : IEntityMetadata;
    }
}