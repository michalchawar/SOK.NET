using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Common.Interface
{
    public interface IAddressRepository : IRepository<Address>
    {
        public Task<Address?> GetFullAsync(Expression<Func<Address, bool>> filter, bool tracked = false);

        public Task<Address?> GetRandomAsync();
    }
}
