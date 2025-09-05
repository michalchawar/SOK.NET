using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.Models.Parish.Entities
{
    /// <summary>
    /// Reprezentuje archiwalny stan danych osoby zg³aszaj¹cej (Submitter) w danym momencie.
    /// Pozwala œledziæ historiê zmian danych kontaktowych zg³aszaj¹cego.
    /// </summary>
    public class SubmitterSnapshot
    {
        /// <summary>
        /// Unikalny identyfikator snapshotu (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator zg³aszaj¹cego w momencie utworzenia snapshotu.
        /// W wiêkszoœci przypadków wszystkie snapshoty dla danego zg³aszaj¹cego bêd¹ mia³y ten sam UniqueId.
        /// </summary>
        public Guid UniqueId { get; set; } = default!;

        /// <summary>
        /// Imiê osoby zg³aszaj¹cej w momencie utworzenia snapshotu.
        /// </summary>
        [MaxLength(64)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Nazwisko osoby zg³aszaj¹cej w momencie utworzenia snapshotu.
        /// </summary>
        [MaxLength(64)]
        public string Surname { get; set; } = default!;

        /// <summary>
        /// Adres e-mail osoby zg³aszaj¹cej w momencie utworzenia snapshotu (opcjonalny).
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; }

        /// <summary>
        /// Numer telefonu osoby zg³aszaj¹cej w momencie utworzenia snapshotu (opcjonalny).
        /// </summary>
        [MaxLength(15)]
        public string? Phone { get; set; }

        /// <summary>
        /// Data i godzina utworzenia snapshotu.
        /// </summary>
        public DateTime ChangeTime { get; private set; }

        /// <summary>
        /// Identyfikator u¿ytkownika, który wprowadzi³ zmianê, nadpisuj¹c dane z tego snapshotu.
        /// </summary>
        public int? ChangeAuthorId { get; set; } = default!;

        /// <summary>
        /// U¿ytkownik, który wprowadzi³ zmianê, nadpisuj¹c dane z tego snapshotu (relacja opcjonalna).
        /// </summary>
        public User? ChangeAuthor { get; set; } = default!;
    }

    public class SubmitterSnapshotEntityTypeConfiguration : IEntityTypeConfiguration<SubmitterSnapshot>
    {
        public void Configure(EntityTypeBuilder<SubmitterSnapshot> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            // (nie ma potrzeby dodatkowych indeksów poza kluczem g³ównym)

            // Generowane pola
            builder.Property(ss => ss.ChangeTime)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            // Relacje
            builder.HasOne(ss => ss.ChangeAuthor)
                .WithMany()
                .HasForeignKey(ss => ss.ChangeAuthorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}