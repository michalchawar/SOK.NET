using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Linq.Expressions;

namespace SOK.Application.Common.Interface
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>? orderBy = null, 
            string? includeProperties = null, 
            bool tracked = false);
        
        Task<T?> GetAsync(
            Expression<Func<T, bool>> filter,
            Expression<Func<T, object>>? orderBy = null, 
            string? includeProperties = null, 
            bool tracked = false);
        
        void Add(T entity);
        
        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
        
        void Remove(T entity);
    }
}