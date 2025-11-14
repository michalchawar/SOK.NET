using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    /// <inheritdoc />
    public class ParishInfoRepository : UpdatableRepository<ParishInfo, ParishDbContext>, IParishInfoRepository
    {
        public ParishInfoRepository(ParishDbContext db) : base(db) {}

        /// <inheritdoc />
        public async Task<string?> GetValueAsync(string name)
        {
            return (await GetQueryable().FirstOrDefaultAsync(pi => pi.Name == name))?.Value;
        }

        /// <inheritdoc />
        public async Task SetValueAsync(string name, string value)
        {
            ParishInfo? parishInfo = await GetQueryable().FirstOrDefaultAsync(pi => pi.Name == name);

            if (parishInfo != null)
            {
                parishInfo.Value = value;
                Update(parishInfo);
            }
            else
            {
                ParishInfo newParishInfo = new ParishInfo
                {
                    Name = name,
                    Value = value
                };
                await dbSet.AddAsync(newParishInfo);
            }
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, string>> ToDictionaryAsync()
        {
            return await GetQueryable()
                .ToDictionaryAsync(pi => pi.Name, pi => pi.Value);
        }

        /// <inheritdoc />
        public async Task ClearValueAsync(string name)
        {
            ParishInfo? parishInfo = await GetQueryable().FirstOrDefaultAsync(pi => pi.Name == name);

            if (parishInfo is not null)
            {
                dbSet.Remove(parishInfo);
            }
        }
    }
}
