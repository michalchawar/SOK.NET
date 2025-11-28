using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje log wysłanego lub oczekującego na wysłanie emaila
    /// </summary>
    [Table("EmailLogs")]
    public class EmailLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Adres email nadawcy
        /// </summary>
        public string SenderMail { get; set; } = string.Empty;

        /// <summary>
        /// Nazwa nadawcy
        /// </summary>
        [MaxLength(255)]
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// JSON array z odbiorcami (TO)
        /// </summary>
        [MaxLength(255)]
        public string Receiver { get; set; } = string.Empty;

        /// <summary>
        /// JSON array z odbiorcami CC
        /// </summary>
        public string Cc { get; set; } = "[]";

        /// <summary>
        /// JSON array z odbiorcami BCC
        /// </summary>
        public string Bcc { get; set; } = "[]";

        /// <summary>
        /// Temat emaila
        /// </summary>
        [MaxLength(500)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Treść emaila (HTML)
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Typ szablonu emaila (np. "confirmation", "data_change", "blank")
        /// </summary>
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Data i czas dodania emaila do kolejki
        /// </summary>
        public DateTime AddedTimestamp { get; private set; }

        /// <summary>
        /// Czy email został wysłany
        /// </summary>
        public bool Sent { get; set; } = false;

        /// <summary>
        /// Priorytet wysyłki (0-10, wyższe liczby = wyższy priorytet)
        /// </summary>
        public int Priority { get; set; } = 5;

        /// <summary>
        /// Data i czas ostatniej aktualizacji statusu (np. wysłania)
        /// </summary>
        public DateTime SentTimestamp { get; set; }

        /// <summary>
        /// Powiązane zgłoszenie (opcjonalne)
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// Powiązane zgłoszenie (relacja nawigacyjna).
        /// </summary>
        public Submission Submission { get; set; } = default!;
    }
}
