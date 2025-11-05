namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje przypisanie budynku do dnia i harmonogramu.
    /// Pozwala powi¹zaæ konkretny budynek z dniem w ramach danego harmonogramu (Schedule).
    /// Przypisania wykorzystywane s¹ do automatycznego przypisywania wizyt do danych dni
    /// oraz sugerowania agend przy planowaniu wizyt. Aby dane przypisanie by³o wtedy brane pod uwagê,
    /// harmonogram wizyty musi byæ zgodny z harmonogramem przypisania (oraz adres wizyty musi byæ
    /// zgodny z adresem budynku).
    /// </summary>
    public class BuildingAssignment
    {
        /// <summary>
        /// Identyfikator dnia, do którego przypisany jest budynek.
        /// </summary>
        public int DayId { get; set; }

        /// <summary>
        /// Dzieñ, do którego przypisany jest budynek (relacja nawigacyjna).
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
        /// Identyfikator harmonogramu, w ramach którego nastêpuje przypisanie.
        /// </summary>
        public int ScheduleId { get; set; }

        /// <summary>
        /// Harmonogram, w ramach którego nastêpuje przypisanie budynku (relacja nawigacyjna).
        /// </summary>
        public Schedule Schedule { get; set; } = default!;
    }
}