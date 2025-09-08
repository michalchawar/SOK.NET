using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Central
{
    /// <summary>
    /// Reprezentuje parafiê w systemie.
    /// Przechowuje podstawowe dane dla ka¿dej parafii.
    /// </summary>
    public class ParishEntry
    {
        /// <summary>
        /// Unikalny identyfikator parafii (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator parafii (unikalny w systemie).
        /// </summary>
        public Guid UniqueId { get; set; } = default!;

        /// <summary>
        /// Nazwa parafii, widoczna dla administratora.
        /// </summary>
        [MaxLength(256)]
        public string ParishName { get; set; } = default!;

        /// <summary>
        /// Zaszyfrowany ConnectionString dla bazy indywidualnej.
        /// </summary>
        [MaxLength(1024)]
        public string EncryptedConnectionString { get; set; } = default!;

        /// <summary>
        /// Data i godzina utworzenia parafii w systemie.
        /// </summary>
        public DateTime CreationTime { get; private set; }

        /// <summary>
        /// Nazwa ostatniej zastosowanej migracji.
        /// </summary>
        public string? MigrationVersion { get; set; }

        /// <summary>
        /// Lista u¿ytkowników, którzy s¹ przypisani do tej parafii.
        /// </summary>
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}