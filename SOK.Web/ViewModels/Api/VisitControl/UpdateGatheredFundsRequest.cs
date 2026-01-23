namespace SOK.Web.ViewModels.Api.VisitControl
{
    /// <summary>
    /// Request do aktualizacji zebranych funduszy.
    /// </summary>
    public class UpdateGatheredFundsRequest
    {
        public Guid AgendaUniqueId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public float GatheredFunds { get; set; }
    }
}
