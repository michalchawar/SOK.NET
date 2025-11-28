using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    /// <summary>
    /// Repozytorium dla EmailLog
    /// </summary>
    public class EmailLogRepository : UpdatableRepository<EmailLog, ParishDbContext>, IEmailLogRepository
    {
        public EmailLogRepository(ParishDbContext context) : base(context)
        {
        }
    }
}
