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
    }
}