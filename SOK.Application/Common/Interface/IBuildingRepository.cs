using System.Linq.Expressions;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium budynków (bram i domów).
    /// </summary>
    public interface IBuildingRepository :  IUpdatableRepository<Building>
    {
        Task<IEnumerable<Building>> GetPaginatedAsync(
            Expression<Func<Building, bool>>? filter,
            int pageSize = 1,
            int page = 1,
            bool street = false,
            bool addresses = false,
            bool days = false,
            bool tracked = false);
    }
}
