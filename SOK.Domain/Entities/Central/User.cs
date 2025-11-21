using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Central
{
    /// <summary>
    /// Reprezentuje użytkownika systemu i jego przynależność.
    /// Przechowuje dane do identyfikacji (login, e-mail) i unikatowy identyfikator parafii, do której należy.
    /// </summary>
    public class User : IdentityUser
    {

        /// <summary>
        /// <summary>
        /// Nazwa wyświetlana użytkownika (np. imię i nazwisko).
        /// </summary>
        [MaxLength(128)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Identyfikator parafii, do której przypisany jest użytkownik.
        /// </summary>
        public int ParishId { get; set; }

        /// <summary>
        /// Parafia, do której przypisany jest użytkownik (relacja nawigacyjna).
        /// </summary>
        public ParishEntry Parish { get; set; } = default!;
    }
}