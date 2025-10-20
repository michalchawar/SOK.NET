using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    public class VisitRepository : Repository<Visit, ParishDbContext>, IVisitRepository
    {
        private readonly ParishDbContext _db;
        public VisitRepository(ParishDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Visit visit)
        {
            dbSet.Update(visit);
        }
    }
}
