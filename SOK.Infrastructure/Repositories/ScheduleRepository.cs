using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOK.Infrastructure.Repositories
{
    /// <inheritdoc />
    public class ScheduleRepository : UpdatableRepository<Schedule, ParishDbContext>, IScheduleRepository
    {
        public ScheduleRepository(ParishDbContext db) : base(db) {}
    }
}
