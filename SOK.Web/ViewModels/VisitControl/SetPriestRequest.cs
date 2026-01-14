namespace SOK.Web.ViewModels.VisitControl
{
    /// <summary>
    /// Request do ustawiania księża prowadzącego wizytę.
    /// </summary>
    public class SetPriestRequest
    {
        public Guid AgendaUniqueId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public int PriestId { get; set; }
    }
}
