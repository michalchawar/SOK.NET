using SOK.Application.Common.DTO;
using SOK.Domain.Entities.Central;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.Linq.Expressions;
using System.Security.Claims;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do obsługi obiektów <see cref="ParishMember"/>.
    /// </summary>
    /// <remarks>
    /// Usługa operuje na obiektach reprezentujących członków parafii,
    /// którzy dopiero dalej są połączeni z rzeczywistymi użytkownikami systemu 
    /// (poprzez pole <see cref="ParishMember.CentralUserId"/>).
    /// </remarks>
    public interface IParishMemberService
    {
        /// <summary>
        /// Pobiera użytkownika o podanym identifykatorze.
        /// </summary>
        /// <param name="id">Identyfikator użytkownika do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="ParishMember"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeśli użytkownik o podanym identyfikatorze nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<ParishMember?> GetParishMemberAsync(int id);

        /// <summary>
        /// Pobiera użytkownika na podstawie podanego <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="userClaim">Zbiór oświadczeń użytkownika.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="ParishMember"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeśli użytkownik o identyfikatorze zawartym w oświadczeniu nie istnieje,
        /// lub jeśli nie znaleziono oświadczenia z identyfikatorem, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<ParishMember?> GetParishMemberAsync(ClaimsPrincipal userClaim);

        /// <summary>
        /// Pobiera stronę użytkowników spełniających podany filtr.
        /// </summary>
        /// <param name="filter">Filtr, który spełniać mają użytkownicy.</param>
        /// <param name="page">Numer strony.</param>
        /// <param name="pageSize">Liczba obiektów na stronie.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="ParishMember"/>.
        /// </returns>
        /// <remarks>
        /// Jeśli nie jest ustawiony żaden filtr, funkcja zwraca wszystkich użytkowników podzielonych na strony.
        /// Jeśli zaś nie ma żadnego użytkownika lub filtr nie pasuje do żadnego użytkownika, zwracana jest pusta lista.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Jeśli podany numer strony lub rozmiar strony jest mniejszy niż 1.
        /// </exception>
        // Task<IEnumerable<ParishMember>> GetParishMembersPaginatedAsync(
        //     Expression<Func<ParishMember, bool>>? filter = null,
        //     int page = 1,
        //     int pageSize = 1);

        /// <summary>
        /// Pobiera listę użytkowników, spełniających podany filtr.
        /// </summary>
        /// <param name="filter">Filtr, który spełniać mają użytkownicy.</param>
        /// <param name="agendas">Określa, czy należy załadować powiązane agendy (do których użytkownik jest przypisany).</param>
        /// <param name="plans">Określa, czy należy załadować plany, do których użytkownik jest przypisany.</param>
        /// <param name="submissions">Określa, czy należy załadować zgłoszenia, które użytkownik utworzył.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="ParishMember"/>.
        /// </returns>
        /// <remarks>
        /// Jeśli nie jest ustawiony żaden filtr, funkcja zwraca wszystkich użytkowników.
        /// Jeśli zaś nie ma żadnego użytkownika lub filtr nie pasuje do żadnego użytkownika, zwracana jest pusta lista.
        /// </remarks>
        Task<IEnumerable<ParishMember>> GetAllParishMembersAsync(
            Expression<Func<ParishMember, bool>>? filter = null,
            bool agendas = false,
            bool plans = false,
            bool submissions = false);

        /// <summary>
        /// Zapisuje użytkownika w bazie danych.
        /// </summary>
        /// <param name="parishMember">Użytkownik, który ma zostać zapisany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Jeśli użytkownik o podanym loginie już istnieje.
        /// </exception>
        //Task CreateParishMemberAsync(ParishMember parishMember);

        /// <summary>
        /// Usuwa użytkownika o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Id użytkownika, który ma zostać usunięty.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest wartość logiczna określająca, czy usunięcie się powiodło.
        /// </returns>
        //Task<bool> DeleteParishMemberAsync(int id);

        /// <summary>
        /// Aktualizuje użytkownika w bazie danych.
        /// </summary>
        /// <param name="parishMember">Użytkownik, który ma zostać zaktualizowany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task UpdateParishMemberAsync(ParishMember parishMember);

        /// <summary>
        /// Pobiera wszystkich użytkowników w podanej roli.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="ParishMember"/> w podanej roli.
        /// </returns>
        Task<IEnumerable<ParishMember>> GetAllInRoleAsync(Role role);

        /// <summary>
        /// Pobiera stronę użytkowników, spełniających podany filtr.
        /// </summary>
        /// <param name="filter">Filtr, który spełniać mają użytkownicy.</param>
        /// <param name="page">Numer strony.</param>
        /// <param name="pageSize">Liczba obiektów na stronie.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="UserDto"/>.
        /// </returns>
        /// <remarks>
        /// Jeśli nie jest ustawiony żaden filtr, funkcja zwraca wszystkich użytkowników podzielonych na strony.
        /// Jeśli zaś nie ma żadnego użytkownika lub filtr nie pasuje do żadnego użytkownika, zwracana jest pusta lista.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Jeśli podany numer strony lub rozmiar strony jest mniejszy niż 1.
        /// </exception>
        Task<List<UserDto>> GetUsersPaginatedAsync(
            Expression<Func<User, bool>>? filter = null,
            int page = 1,
            int pageSize = 1,
            bool loadRoles = false);
    }
}