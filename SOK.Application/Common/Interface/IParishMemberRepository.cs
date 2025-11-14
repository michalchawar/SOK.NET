using SOK.Domain.Entities.Central;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.Linq.Expressions;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium członków parafii.
    /// </summary>
    public interface IParishMemberRepository : IUpdatableRepository<ParishMember>
    {
        Task<IEnumerable<ParishMember>> GetPaginatedAsync(
            Expression<Func<ParishMember, bool>>? filter,
            int pageSize = 1,
            int page = 1,
            bool assignedAgendas = false,
            bool enteredSubmissions = false,
            bool tracked = false);

        /// <summary>
        /// Tworzy nowego członka parafii wraz z kontem w centralnej bazie danych.
        /// </summary>
        /// <param name="displayName">Wyświetlana nazwa użytkownika, który ma zostać utworzony.</param>
        /// <param name="roles">Role, do których użytkownik ma być przypisany.</param>
        /// <returns>
        /// Obiekt <see cref="Task">, reprezentujący asynchroniczną operację, którego wartością jest
        /// obiekt <see cref="ParishMember"/>, jeśli udało się utworzyć nowego użytkownika i członka,
        /// lub <see cref="null"/> w przeciwnym przypadku.
        /// </returns>
        Task<ParishMember?> CreateMemberWithUserAccountAsync(string displayName, IEnumerable<Role> roles);
    }
}
