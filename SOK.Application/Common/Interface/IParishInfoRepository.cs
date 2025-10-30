using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Interface
{
    public interface IParishInfoRepository : IRepository<ParishInfo>
    {
        void Update(ParishInfo parishInfo);

        Task<string?> GetValueAsync(string name);
        
        Task SetValueAsync(string name, string value);

        Task ClearValueAsync(string name);

        Task<Dictionary<string, string>> ToDictionaryAsync();
    }
}