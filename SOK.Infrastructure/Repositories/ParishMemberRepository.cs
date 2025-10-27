using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    public class ParishMemberRepository : Repository<ParishMember, ParishDbContext>, IParishMemberRepository
    {
        private readonly ParishDbContext _db;

        public ParishMemberRepository(ParishDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ParishMember parishMember)
        {
            dbSet.Update(parishMember);
        }
    }
}
