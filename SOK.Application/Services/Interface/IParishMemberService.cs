using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Us³uga do obs³ugi obiektów <see cref="ParishMember"/>.
    /// </summary>
    /// <remarks>
    /// Us³uga operuje na obiektach reprezentuj¹cych cz³onków parafii,
    /// którzy dopiero dalej s¹ po³¹czeni z rzeczywistymi u¿ytkownikami systemu 
    /// (poprzez pole <see cref="ParishMember.CentralUserId"/>).
    /// </remarks>
    public interface IParishMemberService
    {
        /// <summary>
        /// Pobiera u¿ytkownika o podanym identifykatorze.
        /// </summary>
        /// <param name="id">Identyfikator u¿ytkownika do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹,
        /// którego zawartoœci¹ jest obiekt <see cref="ParishMember"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeœli u¿ytkownik o podanym identyfikatorze nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<ParishMember?> GetParishMemberAsync(int id);

        /// <summary>
        /// Pobiera stronê u¿ytkowników spe³niaj¹cych podany filtr.
        /// </summary>
        /// <param name="filter">Filtr, który spe³niaæ maj¹ u¿ytkownicy.</param>
        /// <param name="page">Numer strony.</param>
        /// <param name="pageSize">Liczba obiektów na stronie.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹,
        /// którego zawartoœci¹ jest lista obiektów <see cref="ParishMember"/>.
        /// </returns>
        /// <remarks>
        /// Jeœli nie jest ustawiony ¿aden filtr, funkcja zwraca wszystkich u¿ytkowników podzielonych na strony.
        /// Jeœli zaœ nie ma ¿adnego u¿ytkownika lub filtr nie pasuje do ¿adnego u¿ytkownika, zwracana jest pusta lista.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Jeœli podany numer strony lub rozmiar strony jest mniejszy ni¿ 1.
        /// </exception>
        Task<IEnumerable<ParishMember>> GetParishMembersPaginatedAsync(
            Expression<Func<ParishMember, bool>>? filter = null,
            int page = 1,
            int pageSize = 1);

        /// <summary>
        /// Zapisuje u¿ytkownika w bazie danych.
        /// </summary>
        /// <param name="parishMember">U¿ytkownik, który ma zostaæ zapisany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Jeœli u¿ytkownik o podanym loginie ju¿ istnieje.
        /// </exception>
        //Task CreateParishMemberAsync(ParishMember parishMember);

        /// <summary>
        /// Usuwa u¿ytkownika o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Id u¿ytkownika, który ma zostaæ usuniêty.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹,
        /// którego zawartoœci¹ jest wartoœæ logiczna okreœlaj¹ca, czy usuniêcie siê powiod³o.
        /// </returns>
        //Task<bool> DeleteParishMemberAsync(int id);

        /// <summary>
        /// Aktualizuje u¿ytkownika w bazie danych.
        /// </summary>
        /// <param name="parishMember">U¿ytkownik, który ma zostaæ zaktualizowany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹.
        /// </returns>
        Task UpdateParishMemberAsync(ParishMember parishMember);
    }
}