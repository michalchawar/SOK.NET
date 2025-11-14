using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using System.Linq.Expressions;

namespace SOK.Infrastructure.Repositories
{
    /// <summary>
    /// Reprezentuje repozytorium dla jednego typu encji z dodatkową możliwością manualnej aktualizacji obiektu.
    /// </summary>
    /// <typeparam name="T">Typ encji, którą ma obsługiwać repozytorium.</typeparam>
    public class UpdatableRepository<T, U> : Repository<T, U>, IUpdatableRepository<T> where T : class where U : DbContext
    {
        public UpdatableRepository(U db) : base(db) {}

        /// <inheritdoc />
        public void Update(T entity)
        {
            dbSet.Update(entity);
        }
    }
}
