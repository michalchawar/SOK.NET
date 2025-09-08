using Microsoft.AspNetCore.Identity;
using SOK.Domain.Entities.Central;
using SOK.Domain.Enums;

namespace SOK.Infrastructure.Extensions
{
    /// <summary>
    /// Klasa do rozszerzenia funkcjonalności związanych z zarządzaniem użytkownikami.
    /// </summary>
    public static class UserManagerExtensions
    {
        /// <summary>
        /// Dodaje określonego <paramref name="user"/> do podanej roli.
        /// </summary>
        /// <param name="user">Użytkownik, który zostanie dodany do roli.</param>
        /// <param name="role">Rola, do której należy dodać użytkownika.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący asynchroniczną operację, 
        /// zawierający wynik <see cref="IdentityResult"/> operacji.
        /// </returns>
        public static async Task<IdentityResult> AddToRoleAsync(
            this UserManager<User> userManager,
            User user,
            Role role)
        {
            return await userManager.AddToRoleAsync(user, role.ToString());
        }

        /// <summary>
        /// Dodaje określonego <paramref name="user"/> do podanych ról.
        /// </summary>
        /// <param name="user">Użytkownik, który zostanie dodany do roli.</param>
        /// <param name="roles">Role, do których należy dodać użytkownika.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący asynchroniczną operację, 
        /// zawierający wynik <see cref="IdentityResult"/> operacji.
        /// </returns>
        public static async Task<IdentityResult> AddToRolesAsync(
            this UserManager<User> userManager,
            User user,
            IEnumerable<Role> roles)
        {
            return await userManager.AddToRolesAsync(user, roles.Select(role => role.ToString()));
        }

        /// <summary>
        /// Usuwa określonego <paramref name="user"/> z podanej roli.
        /// </summary>
        /// <param name="user">Użytkownik, który zostanie usunięty z roli.</param>
        /// <param name="role">Rola, z której należy usunąć użytkownika.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący asynchroniczną operację, 
        /// zawierający wynik <see cref="IdentityResult"/> operacji.
        /// </returns>
        public static async Task<IdentityResult> RemoveFromRoleAsync(
            UserManager<User> userManager,
            User user,
            Role role)
        {
            return await userManager.RemoveFromRoleAsync(user, role.ToString());
        }

        /// <summary>
        /// Usuwa określonego <paramref name="user"/> z podanych ról.
        /// </summary>
        /// <param name="user">Użytkownik, który zostanie usunięty z ról.</param>
        /// <param name="roles">Role, z których należy usunąć użytkownika.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący asynchroniczną operację, 
        /// zawierający wynik <see cref="IdentityResult"/> operacji.
        /// </returns>
        public static async Task<IdentityResult> RemoveFromRolesAsync(
            this UserManager<User> userManager,
            User user,
            IEnumerable<Role> roles)
        {
            return await userManager.RemoveFromRolesAsync(user, roles.Select(role => role.ToString()));
        }

        /// <summary>
        /// Pobiera listę ról, do których należy określony <paramref name="user"/>.
        /// </summary>
        /// <param name="user">Użytkownik, którego role należy zwrócić.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący asynchroniczną operację, 
        /// zawierający listę ról użytkownika <paramref name="user"/>.
        /// </returns>
        public static async Task<IList<Role>> GetRolesAsync(
            UserManager<User> userManager,
            User user)
        {
            var roles = await userManager.GetRolesAsync(user);
            return roles.Select(roleName => Enum.Parse<Role>(roleName)).ToList();
        }

        /// <summary>
        /// Zwraca wartość wskazującą, czy określony <paramref name="user"/> należy do podanej <paramref name="role"/>.
        /// </summary>
        /// <param name="user">Użytkownik, którego rola powinna zostać sprawdzona.</param>
        /// <param name="role">Rola, którą należy sprawdzić.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący asynchroniczną operację, 
        /// zawierający wartość wskazującą, czy określony <paramref name="user"/> 
        /// należy do podanej <paramref name="role"/>.
        /// </returns>
        public static async Task<bool> IsInRoleAsync(
            this UserManager<User> userManager,
            User user,
            Role role)
        {
            return await userManager.IsInRoleAsync(user, role.ToString());
        }
    }
}
