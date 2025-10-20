using Microsoft.EntityFrameworkCore;
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
    public class SubmitterRepository : Repository<Submitter, ParishDbContext>, ISubmitterRepository
    {
        private readonly ParishDbContext _db;

        public SubmitterRepository(ParishDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Submitter?> GetRandomAsync()
        {
            var query = GetQueryable(null, null, false);;

            var rand = new Random();
            var skipCount = (int)(rand.NextDouble() * dbSet.Count());

            query = query.Skip(skipCount);

            return await query.FirstOrDefaultAsync();
        }

        public void Update(Submitter submitter)
        {
            dbSet.Update(submitter);
        }
    }
}
