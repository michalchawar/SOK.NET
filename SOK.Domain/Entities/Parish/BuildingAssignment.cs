namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje przypisanie budynku do dnia i harmonogramu.
    /// Pozwala powiązać konkretny budynek z dniem w ramach danego harmonogramu (Schedule).
    /// Przypisania wykorzystywane są do automatycznego przypisywania wizyt do danych dni
    /// oraz sugerowania agend przy planowaniu wizyt. Aby dane przypisanie było wtedy brane pod uwagę,
    /// harmonogram wizyty musi być zgodny z harmonogramem przypisania (oraz adres wizyty musi być
    /// zgodny z adresem budynku).
    /// </summary>
    public class BuildingAssignment
    {
        /// <summary>
        /// Identyfikator dnia, do którego przypisany jest budynek.
        /// </summary>
        public int DayId { get; set; }

        /// <summary>
        /// Dzień, do którego przypisany jest budynek (relacja nawigacyjna).
        /// </summary>
        public Day Day { get; set; } = default!;

        /// <summary>
        /// Identyfikator budynku, który jest przypisany.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Budynek, który jest przypisany do dnia (relacja nawigacyjna).
        /// </summary>
        public Building Building { get; set; } = default!;

        /// <summary>
        /// Identyfikator harmonogramu, w ramach którego następuje przypisanie.
        /// </summary>
        public int ScheduleId { get; set; }

        /// <summary>
        /// Harmonogram, w ramach którego następuje przypisanie budynku (relacja nawigacyjna).
        /// </summary>
        public Schedule Schedule { get; set; } = default!;
    }
}