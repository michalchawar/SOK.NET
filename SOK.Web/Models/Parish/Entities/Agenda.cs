using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;

namespace SOK.Web.Models.Parish.Entities
{
    /// <summary>
    /// Reprezentuje agendê, czyli planowane wizyty w danym dniu dla jednego ksiêdza.
    /// Agenda grupuje wizyty przypisane do konkretnego dnia i u¿ytkownika (np. ksiêdza lub osoby wspieraj¹cej).
    /// Pozwala na zarz¹dzanie harmonogramem, przypisaniami budynków oraz u¿ytkowników.
    /// </summary>
    public class Agenda
    {
        /// <summary>
        /// Unikalny identyfikator agendy (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator agendy (GUID), wykorzystywany g³ównie do udostêpniania lub autoryzacji (w po³¹czeniu z AccessToken).
        /// </summary>
        public Guid UniqueId { get; set; } = default!;

        /// <summary>
        /// Token dostêpu do agendy, u¿ywany do autoryzacji w po³¹czeniu z UniqueId.
        /// </summary>
        [MaxLength(64)]
        public string AccessToken { get; set; } = default!;

        /// <summary>
        /// Czas rozpoczêcia agendy (jeœli inny ni¿ domyœlny dla danego dnia).
        /// </summary>
        public TimeOnly? StartHourOverride { get; set; }

        /// <summary>
        /// Czas zakoñczenia agendy (jeœli inny ni¿ domyœlny dla danego dnia).
        /// </summary>
        public TimeOnly? EndHourOverride { get; set; }

        /// <summary>
        /// Suma zebranych funduszy podczas wizyt w ramach tej agendy.
        /// </summary>
        public float? GatheredFunds { get; set; }

        /// <summary>
        /// Okreœla, czy przypisania wizyt w tej agendzie maj¹ byæ widoczne dla zg³aszaj¹cych (niezalogowanych u¿ytkowników).
        /// Wartoœæ automatycznie ustawiana jest na true na okreœlony (w ustawieniach) czas przed rozpoczêciem agendy.
        /// </summary>
        [DefaultValue(false)]
        public bool ShowsAssignment { get; set; }

        /// <summary>
        /// Okreœla, czy przewidywane godziny wizyt w tej agendzie maj¹ byæ widoczne dla zg³aszaj¹cych (niezalogowanych u¿ytkowników).
        /// Wartoœæ automatycznie ustawiana jest na true na okreœlony (w ustawieniach) czas przed rozpoczêciem agendy.
        /// </summary>
        [DefaultValue(false)]
        public bool ShowHours { get; set; }

        /// <summary>
        /// Identyfikator dnia, do którego przypisana jest agenda.
        /// </summary>
        public int DayId { get; set; }

        /// <summary>
        /// Dzieñ, do którego przypisana jest agenda (relacja nawigacyjna).
        /// </summary>
        public Day Day { get; set; } = default!;

        /// <summary>
        /// Lista wizyt w agendzie. Przy pobieraniu nale¿y j¹ posortowaæ po polu OrdinalNumber w modelu Visit.
        /// </summary>
        public ICollection<Visit> Visits { get; set; } = new List<Visit>();

        /// <summary>
        /// Lista przypisañ budynków powi¹zanych z agend¹. To klasa pomocnicza relacji wiele-do-wielu miêdzy agend¹ a budynkami.
        /// </summary>
        public ICollection<BuildingAssignment> BuildingAssignments { get; set; } = new List<BuildingAssignment>();

        /// <summary>
        /// Lista budynków przypisanych do agendy. Ka¿dy budynek mo¿e byæ przypisany do wielu agend.
        /// </summary>
        public ICollection<Building> BuildingsAssigned { get; set; } = new List<Building>();

        /// <summary>
        /// Lista u¿ytkowników przypisanych do agendy. Agenda mo¿e nie mieæ przypisanych u¿ytkowników. 
        /// U¿ytkownicy mog¹ mieæ ró¿ne role.
        /// </summary>
        public ICollection<User> AssignedUsers { get; set; } = new List<User>();
    }

    public class AgendaEntityTypeConfiguration : IEntityTypeConfiguration<Agenda>
    {
        public void Configure(EntityTypeBuilder<Agenda> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(a => a.UniqueId)
                .IsUnique();

            // Generowane pola
            builder.Property(a => a.AccessToken)
                .HasDefaultValueSql("CONVERT(varchar(64), HASHBYTES('SHA2_256', CAST(NEWID() AS varchar(36))), 2)");

            // Relacje
            builder.HasOne(a => a.Day)
                .WithMany(d => d.Agendas)
                .HasForeignKey(a => a.DayId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.AssignedUsers)
                .WithMany(u => u.AssignedAgendas);
        }
    }
}