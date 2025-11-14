using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium planów.
    /// </summary>
    public interface IPlanRepository : IUpdatableRepository<Plan>
    {
        Task<IEnumerable<Plan>> GetPaginatedAsync(
            Expression<Func<Plan, bool>>? filter,
            int pageSize = 1,
            int page = 1,
            bool author = false, 
            bool schedules = false,
            bool submissions = false,
            bool days = false,
            bool tracked = false);
    }
}
