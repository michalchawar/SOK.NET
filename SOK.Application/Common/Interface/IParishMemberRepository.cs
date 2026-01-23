using SOK.Application.Common.DTO.ParishMember;
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
        Task<IEnumerable<UserDto>> GetPaginatedAsync(
            Expression<Func<User, bool>>? filter,
            int pageSize = 1,
            int page = 1,
            bool roles = false,
            bool assignedAgendas = false,
            bool assignedPlans = false,
            bool enteredSubmissions = false);

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

        /// <summary>
        /// Pobiera użytkownika centralnego (User) po jego Id.
        /// </summary>
        /// <param name="userId">Identyfikator użytkownika centralnego.</param>
        /// <returns>
        /// Obiekt <see cref="Task">, reprezentujący asynchroniczną operację, którego wartością jest
        /// obiekt <see cref="User"/>, lub <see cref="null"/>, jeśli użytkownik nie istnieje.
        /// </returns>
        Task<User?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Aktualizuje dane użytkownika centralnego (User).
        /// </summary>
        /// <param name="user">Użytkownik do zaktualizowania.</param>
        /// <returns>Obiekt <see cref="Task">, reprezentujący asynchroniczną operację.</returns>
        Task UpdateUserAsync(User user);

        /// <summary>
        /// Ustawia nowe hasło dla użytkownika.
        /// </summary>
        /// <param name="user">Użytkownik, dla którego ustawiamy hasło.</param>
        /// <param name="newPassword">Nowe hasło.</param>
        /// <returns>
        /// Obiekt <see cref="Task">, reprezentujący asynchroniczną operację, którego wartością jest
        /// wynik operacji <see cref="IdentityResult"/>.
        /// </returns>
        Task<Microsoft.AspNetCore.Identity.IdentityResult> SetPasswordAsync(User user, string newPassword);
    }
}
