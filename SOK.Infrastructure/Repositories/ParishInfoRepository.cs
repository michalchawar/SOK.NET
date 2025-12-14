using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Interfaces;
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

            if (parishInfo is not null)
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

        /// <inheritdoc />
        public Task<Dictionary<string, string>> GetValuesAsDictionaryAsync(IEnumerable<string> options)
        {
            return GetQueryable()
                .Where(pi => options.Contains(pi.Name))
                .ToDictionaryAsync(pi => pi.Name, pi => pi.Value);
        }

        /// <summary>
        /// Tworzy klucz w formacie: EntityName:EntityId:PropertyName
        /// </summary>
        private string CreateKey<T>(int entityId, string propertyName) where T : IEntityMetadata
        {
            return $"{typeof(T).Name}:{entityId}:{propertyName}";
        }

        /// <inheritdoc />
        public async Task SetMetadataAsync<T>(T entity, string propertyName, string value) 
            where T : IEntityMetadata
        {
            string key = CreateKey<T>(entity.Id, propertyName);
            ParishInfo? existing = await GetQueryable(tracked: true).FirstOrDefaultAsync(pi => pi.Name == key);

            // Aktualizuj istniejące metadane
            if (existing is not null)
            {
                existing.Value = value;
                return;
            }
            
            // Lub dodaj nowe metadane
            var info = new ParishInfo
            {
                Name = key,
                Value = value
            };
            dbSet.Add(info);
        }

        /// <inheritdoc />
        public async Task<string?> GetMetadataAsync<T>(T entity, string propertyName) 
            where T : IEntityMetadata
        {
            string key = CreateKey<T>(entity.Id, propertyName);
            ParishInfo? info = await GetQueryable().FirstOrDefaultAsync(pi => pi.Name == key);
            return info?.Value;
        }

        /// <inheritdoc />
        public async Task DeleteMetadataAsync<T>(T entity, string propertyName) 
            where T : IEntityMetadata
        {
            string key = CreateKey<T>(entity.Id, propertyName);
            ParishInfo? existing = await GetQueryable().FirstOrDefaultAsync(pi => pi.Name == key);
            
            if (existing is not null)
            {
                dbSet.Remove(existing);
            }
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, string>> GetAllMetadataAsync<T>(T entity) 
            where T : IEntityMetadata
        {
            string prefix = CreateKey<T>(entity.Id, string.Empty);
            var allInfos = GetQueryable()
                .Where(pi => pi.Name.StartsWith(prefix));
            
            return allInfos.ToDictionary(
                info => info.Name.Substring(prefix.Length),
                info => info.Value
            );
        }
    }
}
