using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Common.Interface
{
    public interface ISubmitterRepository : IRepository<Submitter>
    {
        void Update(Submitter submitter);

        Task<IEnumerable<Submitter>> GetPaginatedAsync(
            Expression<Func<Submitter, bool>>? filter,
            int pageSize = 1,
            int page = 1,
            bool submissions = false,
            bool tracked = false);
    }
}
