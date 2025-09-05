using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace SOK.Infrastructure.Repositories
{
    public class ParishInfoRepository : Repository<ParishInfo, ParishDbContext>, IParishInfoRepository
    {
        private readonly ParishDbContext _db;

        public ParishInfoRepository(ParishDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ParishInfo parish)
        {
            dbSet.Update(parish);
        }

        public async Task<Dictionary<string, string>> ToDictionaryAsync()
        {
            return await GetQueryable()
                .ToDictionaryAsync(pi => pi.Name, pi => pi.Value);
        }
    }
}
