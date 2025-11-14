using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Central;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    /// <inheritdoc />
    public class ParishRepository : UpdatableRepository<ParishEntry, CentralDbContext>, IParishRepository
    {
        public ParishRepository(CentralDbContext db) : base(db) {}
    }
}
