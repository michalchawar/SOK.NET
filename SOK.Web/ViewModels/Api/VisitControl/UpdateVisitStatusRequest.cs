using SOK.Domain.Enums;

namespace SOK.Web.ViewModels.Api.VisitControl
{
    /// <summary>
    /// Request do aktualizacji statusu wizyty.
    /// </summary>
    public class UpdateVisitStatusRequest
    {
        public Guid AgendaUniqueId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public int VisitId { get; set; }
        public VisitStatus Status { get; set; }
        public int? PeopleCount { get; set; }
    }
}
