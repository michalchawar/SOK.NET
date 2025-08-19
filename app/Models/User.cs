using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using app.Models.Enums;

namespace app.Models
{
    /// <summary>
    /// Reprezentuje u¿ytkownika systemu (np. administratora, ksiêdza, ministranta).
    /// Przechowuje dane logowania, status, role oraz powi¹zania z parafi¹ i agendami.
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
        /// Nazwa wyœwietlana u¿ytkownika (np. imiê i nazwisko).
        /// </summary>
        [MaxLength(128)]
        public string DisplayName { get; set; } = default!;

        /// <summary>
        /// Adres e-mail u¿ytkownika (opcjonalny).
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; } = default!;

        /// <summary>
        /// Hash has³a u¿ytkownika.
        /// </summary>
        [MaxLength(256)]
        public string PasswordHash { get; set; } = default!;

        /// <summary>
        /// Okreœla, czy konto u¿ytkownika jest aktywne w systemie.
        /// </summary>
        [DefaultValue(true)]
        public bool IsActive { get; set; }

        /// <summary>
        /// Zbiór ról przypisanych u¿ytkownikowi.
        /// </summary>
        public HashSet<Role> Roles { get; set; } = new HashSet<Role>();

        /// <summary>
        /// Lista agend, do których u¿ytkownik jest przypisany (np. jako ksi¹dz lub ministrant).
        /// </summary>
        public ICollection<Agenda> AssignedAgendas { get; set; } = new List<Agenda>();

        /// <summary>
        /// Lista zg³oszeñ, które u¿ytkownik wprowadzi³ manualnie.
        /// </summary>
        public ICollection<FormSubmission> EnteredSubmissions { get; set; } = new List<FormSubmission>();
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
            // (User nie jest podrzêdny wzglêdem ¿adnej encji, nie konfigurujemy relacji)
        }
    }
}