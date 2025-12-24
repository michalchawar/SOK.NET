namespace SOK.Web.ViewModels.VisitControl
{
    /// <summary>
    /// Request do dodawania nowego adresu podczas przeprowadzania wizyty.
    /// </summary>
    public class AddAddressRequest
    {
        public Guid AgendaUniqueId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public int ScheduleId { get; set; }
        public int BuildingId { get; set; }
        public string Apartment { get; set; } = string.Empty;
    }
}
