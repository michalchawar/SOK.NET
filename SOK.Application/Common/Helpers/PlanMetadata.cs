namespace SOK.Application.Common.Helpers
{
    /// <summary>
    /// Statyczna klasa definiująca dostępne metadane dla planów.
    /// </summary>
    public static class PlanMetadataKeys
    {
        /// <summary>
        /// Data rozpoczęcia zbierania zgłoszeń.
        /// </summary>
        public const string SubmissionsStartDate = "SubmissionsStartDate";

        /// <summary>
        /// Data zakończenia zbierania zgłoszeń.
        /// </summary>
        public const string SubmissionsEndDate = "SubmissionsEndDate";

        /// <summary>
        /// Data początku kolędy.
        /// </summary>
        public const string VisitsStartDate = "VisitsStartDate";

        /// <summary>
        /// Data końca kolędy.
        /// </summary>
        public const string VisitsEndDate = "VisitsEndDate";

        /// <summary>
        /// Domyślna godzina rozpoczęcia kolędy od poniedziałku do piątku.
        /// </summary>
        public const string DefaultStartTimeWeekdays = "DefaultStartTimeWeekdays";

        /// <summary>
        /// Domyślna godzina zakończenia kolędy od poniedziałku do piątku.
        /// </summary>
        public const string DefaultEndTimeWeekdays = "DefaultEndTimeWeekdays";

        /// <summary>
        /// Domyślna godzina rozpoczęcia kolędy w sobotę.
        /// </summary>
        public const string DefaultStartTimeSaturday = "DefaultStartTimeSaturday";

        /// <summary>
        /// Domyślna godzina zakończenia kolędy w sobotę.
        /// </summary>
        public const string DefaultEndTimeSaturday = "DefaultEndTimeSaturday";

        /// <summary>
        /// Domyślna godzina rozpoczęcia kolędy w niedzielę.
        /// </summary>
        public const string DefaultStartTimeSunday = "DefaultStartTimeSunday";

        /// <summary>
        /// Domyślna godzina zakończenia kolędy w niedzielę.
        /// </summary>
        public const string DefaultEndTimeSunday = "DefaultEndTimeSunday";
    }

    /// <summary>
    /// Statyczna klasa definiująca dostępne metadane dla ParishMember (księdza).
    /// </summary>
    public static class ParishMemberMetadataKeys
    {
        /// <summary>
        /// Jednostka czasowa dla księdza - liczba minut na jedną wizytę (domyślnie 10).
        /// </summary>
        public const string MinutesPerVisit = "MinutesPerVisit";
    }

    /// <summary>
    /// Statyczna klasa definiująca dostępne metadane dla Agendy.
    /// </summary>
    public static class AgendaMetadataKeys
    {
        /// <summary>
        /// Jednostka czasowa dla agendy - liczba minut na jedną wizytę (nadpisuje wartość księdza).
        /// </summary>
        public const string MinutesPerVisit = "MinutesPerVisit";
    }
}
