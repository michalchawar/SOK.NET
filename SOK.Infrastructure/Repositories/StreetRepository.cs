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
    public class StreetRepository : UpdatableRepository<Street, ParishDbContext>, IStreetRepository
    {
        public StreetRepository(ParishDbContext db) : base(db) {}
    }
}
