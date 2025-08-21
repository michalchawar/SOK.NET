using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace app.Models.Central
{
    /// <summary>
    /// Reprezentuje u¿ytkownika systemu i jego przynale¿noœæ.
    /// Przechowuje dane do identyfikacji (login, e-mail) i unikatowy identyfikator parafii, do której nale¿y.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unikalny identyfikator u¿ytkownika (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Login u¿ytkownika (unikalny w systemie).
        /// </summary>
        [MaxLength(64)]
        public string Login { get; set; } = default!;

        /// <summary>
        /// Adres e-mail u¿ytkownika (opcjonalny).
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; } = default!;

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
            builder.HasIndex(u => u.Login)
                .IsUnique();

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