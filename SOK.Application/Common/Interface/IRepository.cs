using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Linq.Expressions;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium dla jednego typu encji.
    /// </summary>
    /// <typeparam name="T">Typ encji, którą ma obsługiwać repozytorium.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Pobiera wszystkie obiekty, spełniające podany filtr, w określonej kolejności.
        /// </summary>
        /// <param name="filter">Filtr, który mają spełniać obiekty.</param>
        /// <param name="orderBy">Porządek, w którym obiekty mają zostać pobrane.</param>
        /// <param name="includeProperties">Nazwy właściwości nawigacyjnych, które mają zostać załadowane (oddzielone przecinkami).</param>
        /// <param name="tracked">Flaga wskazująca, czy stan obiektów ma być śledzony po pobraniu i automatycznie aktualizowany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację,
        /// którego wartością jest <see cref="IEnumerable"/> obiektów <see cref="T"/>.
        /// </returns>
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>? orderBy = null,
            string? includeProperties = null,
            bool tracked = false);

        /// <summary>
        /// Pobiera pierwszy obiekt, spełniający podany filtr, w określonej kolejności.
        /// </summary>
        /// <param name="filter">Filtr, który ma spełniać obiekt.</param>
        /// <param name="orderBy">Porządek, w którym obiekty mają zostać uwzględnione.</param>
        /// <param name="includeProperties">Nazwy właściwości nawigacyjnych, które mają zostać załadowane (oddzielone przecinkami).</param>
        /// <param name="tracked">Flaga wskazująca, czy stan obiektu ma być śledzony po pobraniu i automatycznie aktualizowany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację,
        /// którego wartością jest pobrany obiekt typu <see cref="T"/>.
        /// </returns>
        Task<T?> GetAsync(
            Expression<Func<T, bool>> filter,
            Expression<Func<T, object>>? orderBy = null,
            string? includeProperties = null,
            bool tracked = false);

        /// <summary>
        /// Dodaje obiekt do repozytorium.
        /// </summary>
        /// <param name="entity">Obiekt, który ma zostać dodany.</param>
        void Add(T entity);

        /// <summary>
        /// Określa, czy w repozytorium znajdują się obiekty, spełniające podany filtr.
        /// </summary>
        /// <param name="filter">Filtr, który mają spełniać obiekty.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację,
        /// którego wartością jest flaga, wskazująca czy w repozytorium znajduje się
        /// przynajmniej jeden obiekt, spełniający podany filtr.
        /// </returns>
        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
        
        /// <summary>
        /// Usuwa obiekt z repozytorium.
        /// </summary>
        /// <param name="entity">Obiekt, który ma zostać usunięty.</param>
        void Remove(T entity);
    }
}