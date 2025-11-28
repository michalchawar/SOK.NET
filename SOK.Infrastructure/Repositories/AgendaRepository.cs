using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    /// <inheritdoc />
    public class AgendaRepository : UpdatableRepository<Agenda, ParishDbContext>, IAgendaRepository
    {
        public AgendaRepository(ParishDbContext db) : base(db) {}
    }
}
