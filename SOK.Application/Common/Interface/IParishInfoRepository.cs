using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Common.Interface
{
    public interface IParishInfoRepository : IRepository<ParishInfo>
    {
        void Update(ParishInfo parishInfo);

        Task<Dictionary<string, string>> ToDictionaryAsync();
    }
}