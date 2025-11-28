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
    public class CityRepository : UpdatableRepository<City, ParishDbContext>, ICityRepository
    {
        public CityRepository(ParishDbContext db) : base(db) {}
    }
}
