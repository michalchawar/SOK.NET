namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Serwis do generowania plików PDF z planami kolędowymi.
    /// </summary>
    public interface IPrintService
    {
        /// <summary>
        /// Generuje PDF z planem dla pojedynczej agendy.
        /// </summary>
        /// <param name="agendaId">Identyfikator agendy.</param>
        /// <param name="agendaIndex">Indeks agendy (numer księży, np. 1, 2, 3...).</param>
        /// <returns>Dane PDF jako tablica bajtów.</returns>
        Task<byte[]> GenerateAgendaPdfAsync(int agendaId, int agendaIndex);

        /// <summary>
        /// Generuje PDF z planami dla wszystkich agend w danym dniu.
        /// </summary>
        /// <param name="dayId">Identyfikator dnia.</param>
        /// <returns>Dane PDF jako tablica bajtów.</returns>
        Task<byte[]> GenerateDayPdfAsync(int dayId);
    }
}
