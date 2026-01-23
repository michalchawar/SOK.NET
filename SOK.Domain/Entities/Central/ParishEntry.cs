using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Central
{
    /// <summary>
    /// Reprezentuje parafię w systemie.
    /// Przechowuje podstawowe dane dla każdej parafii.
    /// </summary>
    public class ParishEntry
    {
        /// <summary>
        /// Unikalny identyfikator parafii (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator parafii (unikalny w systemie).
        /// </summary>
        public Guid UniqueId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nazwa parafii, widoczna dla administratora.
        /// </summary>
        [MaxLength(256)]
        public string ParishName { get; set; } = string.Empty;

        /// <summary>
        /// Zaszyfrowany ConnectionString dla bazy indywidualnej.
        /// </summary>
        [MaxLength(1024)]
        public string EncryptedConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Wersja klucza użytego do zaszyfrowania connection stringa.
        /// Pozwala na rotację kluczy i stopniową migrację danych.
        /// </summary>
        public int KeyVersion { get; set; } = 1;

        /// <summary>
        /// Data i godzina utworzenia parafii w systemie.
        /// </summary>
        public DateTime CreationTime { get; private set; }

        /// <summary>
        /// Lista użytkowników, którzy są przypisani do tej parafii.
        /// </summary>
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}