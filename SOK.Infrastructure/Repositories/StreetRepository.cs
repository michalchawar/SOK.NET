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
    public class StreetRepository : Repository<Street, ParishDbContext>, IStreetRepository
    {
        private readonly ParishDbContext _db;

        public StreetRepository(ParishDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Street street)
        {
            dbSet.Update(street);
        }
    }
}
