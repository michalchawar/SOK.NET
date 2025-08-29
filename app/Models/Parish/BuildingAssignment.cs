using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace app.Models.Parish
{
    /// <summary>
    /// Reprezentuje przypisanie budynku do agendy i harmonogramu.
    /// Pozwala powi¹zaæ konkretny budynek z agend¹ w ramach danego harmonogramu (Schedule).
    /// Przypisania wykorzystywane s¹ do automatycznego przypisywania wizyt do danych agend
    /// oraz sugerowania agend przy planowaniu wizyt. Aby dane przypisanie by³o wtedy brane pod uwagê,
    /// harmonogram wizyty musi byæ zgodny z harmonogramem przypisania (oraz adres wizyty musi byæ
    /// zgodny z adresem budynku).
    /// </summary>
    public class BuildingAssignment
    {
        /// <summary>
        /// Identyfikator agendy, do której przypisany jest budynek.
        /// </summary>
        public int AgendaId { get; set; }

        /// <summary>
        /// Agenda, do której przypisany jest budynek (relacja nawigacyjna).
        /// </summary>
        public Agenda Agenda { get; set; } = default!;

        /// <summary>
        /// Identyfikator budynku, który jest przypisany.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Budynek, który jest przypisany do agendy (relacja nawigacyjna).
        /// </summary>
        public Building Building { get; set; } = default!;

        /// <summary>
        /// Identyfikator harmonogramu, w ramach którego nastêpuje przypisanie.
        /// </summary>
        public int ScheduleId { get; set; }

        /// <summary>
        /// Harmonogram, w ramach którego nastêpuje przypisanie budynku (relacja nawigacyjna).
        /// </summary>
        public Schedule Schedule { get; set; } = default!;
    }

    public class BuildingAssignmentEntityTypeConfiguration : IEntityTypeConfiguration<BuildingAssignment>
    {
        public void Configure(EntityTypeBuilder<BuildingAssignment> builder)
        {
            // Klucz g³ówny
            builder.HasKey(ba => new { ba.AgendaId, ba.BuildingId, ba.ScheduleId });

            // Indeksy i unikalnoœæ
            // (nie ma potrzeby dodatkowych indeksów poza kluczem g³ównym)

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            builder.HasOne(ba => ba.Agenda)
                .WithMany(a => a.BuildingAssignments)
                .HasForeignKey(ba => ba.AgendaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ba => ba.Building)
                .WithMany(b => b.BuildingAssignments)
                .HasForeignKey(ba => ba.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ba => ba.Schedule)
                .WithMany(s => s.BuildingAssignments)
                .HasForeignKey(ba => ba.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}