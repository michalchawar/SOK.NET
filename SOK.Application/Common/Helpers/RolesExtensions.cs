using Microsoft.AspNetCore.Identity;
using SOK.Domain.Entities.Central;
using SOK.Domain.Enums;

namespace SOK.Application.Common.Helpers
{
    /// <summary>
    /// Klasa do rozszerzenia funkcjonalności związanych z enumem ról.
    /// </summary>
    public static class RolesExtensions
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

        /// <summary>
        /// Zwraca listę użytkowników, którzy są w podanej roli.
        /// </summary>
        /// <param name="role">Rola, do której mają należeć użytkownicy.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący asynchroniczną operację, 
        /// zawierający listę obiektów <see cref="User"/>, którzy są w podanej roli.
        /// </returns>
        public static async Task<IList<User>> GetUsersInRoleAsync(
            this UserManager<User> userManager,
            Role role)
        {
            return await userManager.GetUsersInRoleAsync(role.ToString());
        }

        /// <summary>
        /// Zwraca polską nazwę roli.
        /// </summary>
        /// <param name="role">Rola do przetłumaczenia.</param>
        /// <returns>Polska nazwa roli.</returns>
        public static string ToPolishName(this Role role)
        {
            return role switch
            {
                Role.SuperAdmin => "Superadministrator",
                Role.Administrator => "Administrator",
                Role.Priest => "Ksiądz",
                Role.SubmitSupport => "Wsparcie zgłoszeń",
                Role.VisitSupport => "Wsparcie wizyt",
                Role.DefaultUser => "Użytkownik",
                _ => role.ToString()
            };
        }

        /// <summary>
        /// Zwraca klasę koloru badge dla danej roli (dla DaisyUI).
        /// </summary>
        /// <param name="role">Rola, dla której pobieramy klasę koloru.</param>
        /// <returns>Klasa CSS dla badge.</returns>
        public static string GetBadgeColorClass(this Role role)
        {
            return role switch
            {
                Role.SuperAdmin => "bg-rose-600 border-rose-600 text-white",
                Role.Administrator => "bg-red-500 border-red-500 text-white",
                Role.Priest => "bg-purple-500 border-purple-500 text-white",
                Role.SubmitSupport => "bg-blue-500 border-blue-500 text-white",
                Role.VisitSupport => "bg-green-500 border-green-500 text-white",
                Role.DefaultUser => "bg-gray-400 border-gray-400 text-white",
                _ => "bg-gray-300 border-gray-300"
            };
        }

        /// <summary>
        /// Zwraca priorytet sortowania dla roli (niższa wartość = wyższy priorytet).
        /// </summary>
        /// <param name="role">Rola do oceny.</param>
        /// <returns>Wartość priorytetu.</returns>
        public static int GetSortPriority(this Role role)
        {
            return role switch
            {
                Role.SuperAdmin => 0,
                Role.Administrator => 1,
                Role.Priest => 2,
                Role.SubmitSupport => 3,
                Role.VisitSupport => 4,
                Role.DefaultUser => 5,
                _ => 99
            };
        }

        /// <summary>
        /// Zwraca ikonę Bootstrap Icons dla danej roli.
        /// </summary>
        /// <param name="role">Rola, dla której pobieramy ikonę.</param>
        /// <returns>Klasa ikony Bootstrap Icons.</returns>
        public static string GetRoleIcon(this Role role)
        {
            return role switch
            {
                Role.SuperAdmin => "bi-shield-lock-fill",
                Role.Administrator => "bi-shield-fill-check",
                Role.Priest => "bi-book",
                Role.SubmitSupport => "bi-clipboard-check",
                Role.VisitSupport => "bi-house-heart",
                Role.DefaultUser => "bi-person",
                _ => "bi-person-circle"
            };
        }
    }
}
