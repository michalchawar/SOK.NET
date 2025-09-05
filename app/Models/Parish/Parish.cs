using app.Models.Parish.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace app.Models.Parish
{
    /// <summary>
    /// Reprezentuje parafiê, czyli jednostkê organizacyjn¹ Koœcio³a.
    /// Parafia posiada w³asny adres, u¿ytkowników oraz plany wizyt duszpasterskich.
    /// </summary>
    public class Parish
    {
        /// <summary>
        /// Unikalny identyfikator parafii (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator parafii (GUID).
        /// </summary>
        [Required]
        public Guid UniqueId { get; set; } = default!;

        /// <summary>
        /// Pe³na nazwa parafii.
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string FullName { get; set; } = default!;

        /// <summary>
        /// Identyfikator adresu parafii.
        /// </summary>
        [Required]
        public int AddressId { get; set; }
        /// <summary>
        /// Adres parafii (relacja nawigacyjna).
        /// </summary>
        public Address Address { get; set; } = default!;

        /// <summary>
        /// Identyfikator diecezji, do której nale¿y parafia.
        /// </summary>
        [Required]
        public int DioceseId { get; set; }

        /// <summary>
        /// Diecezja, do której nale¿y parafia (relacja nawigacyjna).
        /// </summary>
        public Diocese Diocese { get; set; } = default!;

        /// <summary>
        /// Lista u¿ytkowników przypisanych do parafii.
        /// </summary>
        public ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Lista planów wizyt duszpasterskich utworzonych w parafii.
        /// </summary>
        public ICollection<Plan> Plans { get; set; } = new List<Plan>();
    }
}