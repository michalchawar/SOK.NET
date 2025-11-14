using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium zgłaszających.
    /// </summary>
    public interface ISubmitterRepository : IUpdatableRepository<Submitter>
    {
        Task<IEnumerable<Submitter>> GetPaginatedAsync(
            Expression<Func<Submitter, bool>>? filter,
            int pageSize = 1,
            int page = 1,
            bool submissions = false,
            bool tracked = false);
    }
}
