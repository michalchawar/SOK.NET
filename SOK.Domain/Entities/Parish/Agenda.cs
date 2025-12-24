using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SOK.Domain.Interfaces;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje agendę, czyli planowane wizyty w danym dniu dla jednego księdza.
    /// Agenda grupuje wizyty przypisane do konkretnego dnia i użytkownika (np. księdza lub osoby wspierającej).
    /// Pozwala na zarządzanie harmonogramem, przypisaniami budynków oraz użytkowników.
    /// </summary>
    public class Agenda : IEntityMetadata
    {
        /// <summary>
        /// Unikalny identyfikator agendy (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator agendy (GUID), wykorzystywany głównie do udostępniania lub autoryzacji (w połączeniu z AccessToken).
        /// </summary>
        public Guid UniqueId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Token dostępu do agendy, używany do autoryzacji w połączeniu z UniqueId.
        /// </summary>
        [MaxLength(64)]
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// Czas rozpoczęcia agendy (jeśli inny niż domyślny dla danego dnia).
        /// </summary>
        public TimeOnly? StartHourOverride { get; set; }

        /// <summary>
        /// Czas zakończenia agendy (jeśli inny niż domyślny dla danego dnia).
        /// </summary>
        public TimeOnly? EndHourOverride { get; set; }

        /// <summary>
        /// Suma zebranych funduszy podczas wizyt w ramach tej agendy.
        /// </summary>
        public float? GatheredFunds { get; set; }

        /// <summary>
        /// Określa, czy wizyty zapisane do tej agendy mają być ukryte dla zgłaszających (niezalogowanych użytkowników).
        /// </summary>
        [DefaultValue(false)]
        public bool HideVisits { get; set; } = false;

        /// <summary>
        /// Określa, czy przewidywane godziny wizyt w tej agendzie mają być widoczne dla zgłaszających (niezalogowanych użytkowników).
        /// Wartość automatycznie ustawiana jest na true na określony (w ustawieniach) czas przed rozpoczęciem agendy.
        /// </summary>
        [DefaultValue(false)]
        public bool ShowHours { get; set; } = false;

        /// <summary>
        /// Identyfikator dnia, do którego przypisana jest agenda.
        /// </summary>
        public int DayId { get; set; }

        /// <summary>
        /// Dzień, do którego przypisana jest agenda (relacja nawigacyjna).
        /// </summary>
        public Day Day { get; set; } = default!;

        /// <summary>
        /// Lista wizyt w agendzie. Przy pobieraniu należy ją posortować po polu OrdinalNumber w modelu Visit.
        /// </summary>
        public ICollection<Visit> Visits { get; set; } = new List<Visit>();

        /// <summary>
        /// Lista użytkowników przypisanych do agendy. Agenda może nie mieć przypisanych użytkowników. 
        /// Użytkownicy mogą mieć różne role.
        /// </summary>
        public ICollection<ParishMember> AssignedMembers { get; set; } = new List<ParishMember>();
    }
}