using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Central;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    public class ParishRepository : Repository<ParishEntry, CentralDbContext>, IParishRepository
    {
        private readonly CentralDbContext _db;

        public ParishRepository(CentralDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ParishEntry parish)
        {
            dbSet.Update(parish);
        }
    }
}
