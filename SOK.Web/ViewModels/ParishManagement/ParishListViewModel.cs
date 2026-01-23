namespace SOK.Web.ViewModels.ParishManagement
{
    /// <summary>
    /// Model widoku listy parafii.
    /// </summary>
    public class ParishListViewModel
    {
        /// <summary>
        /// Lista parafii.
        /// </summary>
        public List<ParishItemViewModel> Parishes { get; set; } = new();

        /// <summary>
        /// Unikalny identyfikator aktualnie wybranej parafii.
        /// </summary>
        public string? SelectedParishUid { get; set; }
    }
}
