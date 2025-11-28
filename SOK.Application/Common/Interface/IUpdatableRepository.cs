namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium dla jednego typu encji z dodatkową możliwością manualnej aktualizacji obiektu.
    /// </summary>
    /// <typeparam name="T">Typ encji, którą ma obsługiwać repozytorium.</typeparam>
    public interface IUpdatableRepository<T> : IRepository<T> where T : class
    {
        /// <summary>
        /// Aktualizuje obiekt w repozytorium.
        /// </summary>
        /// <param name="entity">Obiekt, który ma zostać zaktualizowany.</param>
        void Update(T entity);
    }
}