using SOK.Web.Models.Central.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SOK.Web.Models.Central.Entities
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

    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            // (nie ma potrzeby dodatkowych indeksów poza kluczem g³ównym)

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            builder.HasOne(u => u.Parish)
                .WithMany(p => p.Users)
                .HasForeignKey(u => u.ParishId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}