using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    public class AgendaRepository : Repository<Agenda, ParishDbContext>, IAgendaRepository
    {
        private readonly ParishDbContext _db;
        public AgendaRepository(ParishDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Agenda agenda)
        {
            dbSet.Update(agenda);
        }
    }
}
