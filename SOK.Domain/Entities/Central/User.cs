using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Central
{
    /// <summary>
    /// Reprezentuje u¿ytkownika systemu i jego przynale¿noœæ.
    /// Przechowuje dane do identyfikacji (login, e-mail) i unikatowy identyfikator parafii, do której nale¿y.
    /// </summary>
    public class User : IdentityUser
    {

        /// <summary>
        /// <summary>
        /// Nazwa wyœwietlana u¿ytkownika (np. imiê i nazwisko).
        /// </summary>
        [MaxLength(128)]
        public string DisplayName { get; set; } = default!;

        /// <summary>
        /// Identyfikator parafii, do której przypisany jest u¿ytkownik.
        /// </summary>
        public int ParishId { get; set; }

        /// <summary>
        /// Parafia, do której przypisany jest u¿ytkownik (relacja nawigacyjna).
        /// </summary>
        public ParishEntry Parish { get; set; } = default!;
    }
}