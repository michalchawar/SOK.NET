namespace SOK.Web.ViewModels.ParishManagement
{
    /// <summary>
    /// Model widoku pojedynczego elementu parafii.
    /// </summary>
    public class ParishItemViewModel
    {
        /// <summary>
        /// Identyfikator parafii w bazie danych.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unikalny identyfikator parafii (UID).
        /// </summary>
        public string UniqueId { get; set; } = string.Empty;

        /// <summary>
        /// Nazwa parafii.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
