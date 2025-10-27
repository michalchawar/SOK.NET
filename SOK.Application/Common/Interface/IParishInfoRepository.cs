using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Common.Interface
{
    public interface IParishInfoRepository : IRepository<ParishInfo>
    {
        void Update(ParishInfo parishInfo);

        Task<string?> GetValueAsync(string name);

        Task<Dictionary<string, string>> ToDictionaryAsync();
    }
}