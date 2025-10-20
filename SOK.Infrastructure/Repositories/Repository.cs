using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using System.Linq.Expressions;

namespace SOK.Infrastructure.Repositories
{
    public class Repository<T, U> : IRepository<T> where T : class where U : DbContext
    {
        private readonly U _db;
        internal DbSet<T> dbSet;

        public Repository(U db)
        {
            _db = db;
            dbSet = _db.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
        {
            return await dbSet.AnyAsync(filter);
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            return await GetQueryable(filter, includeProperties, tracked)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, bool tracked = false)
        {
            return await GetQueryable(filter, includeProperties, tracked)
                .ToListAsync();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        protected IQueryable<T> GetQueryable(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<T> query = tracked ? dbSet : dbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            return query;
        }
    }
}
