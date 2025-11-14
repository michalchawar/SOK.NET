using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    public class VisitRepository : UpdatableRepository<Visit, ParishDbContext>, IVisitRepository
    {
        public VisitRepository(ParishDbContext db) : base(db) {}
    }
}
