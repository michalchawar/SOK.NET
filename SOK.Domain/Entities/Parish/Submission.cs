using SOK.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje zg³oszenie z³o¿one przez prywatn¹ osobê zg³aszaj¹c¹ 
    /// lub zalogowanego u¿ytkownika w jej imieniu. Zawiera dane powi¹zane z formularzem, 
    /// adres, wszelakie uwagi i ich status, powi¹zania z planem, wizyt¹ i histori¹ zmian.
    /// </summary>
    public class Submission
    {
        /// <summary>
        /// Unikalny identyfikator zg³oszenia (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator zg³oszenia (GUID), wykorzystywany g³ównie do udostêpniania lub autoryzacji (w po³¹czeniu z AccessToken).
        /// </summary>
        public Guid UniqueId { get; set; } = default!;

        /// <summary>
        /// Token dostêpu do zg³oszenia, u¿ywany do autoryzacji w po³¹czeniu z UniqueId.
        /// </summary>
        [MaxLength(64)]
        public string AccessToken { get; set; } = default!;

        /// <summary>
        /// Uwagi zg³aszaj¹cego (opcjonalne).
        /// </summary>
        [MaxLength(512)]
        public string? SubmitterNotes { get; set; }

        /// <summary>
        /// Wiadomoœæ administracyjna skierowana do zg³aszaj¹cego (opcjonalna).
        /// </summary>
        [MaxLength(512)]
        public string? AdminMessage { get; set; }

        /// <summary>
        /// Systemowe notatki administratora dotycz¹ce zg³oszenia (opcjonalne).
        /// </summary>
        [MaxLength(512)]
        public string? AdminNotes { get; set; }

        /// <summary>
        /// Status realizacji uwag od u¿ytkownika.
        /// Status jest równy NotesFulfillmentStatus.NA, jeœli zg³oszenie nie zawiera uwag od u¿ytkownika.
        /// </summary>
        [DefaultValue(NotesFulfillmentStatus.NA)]
        public NotesFulfillmentStatus NotesStatus { get; set; }

        /// <summary>
        /// Identyfikator osoby zg³aszaj¹cej (Submitter).
        /// </summary>
        public int SubmitterId { get; set; } = default!;

        /// <summary>
        /// Osoba zg³aszaj¹ca (relacja nawigacyjna).
        /// </summary>
        public Submitter Submitter { get; set; } = default!;

        /// <summary>
        /// Identyfikator adresu powi¹zanego ze zg³oszeniem.
        /// </summary>
        public int AddressId { get; set; } = default!;

        /// <summary>
        /// Adres powi¹zany ze zg³oszeniem (relacja nawigacyjna).
        /// </summary>
        public Address Address { get; set; } = default!;

        /// <summary>
        /// Powi¹zane archiwalne dane formularza zg³oszeniowego.
        /// </summary>
        public FormSubmission FormSubmission { get; set; } = default!;

        /// <summary>
        /// Wizyta powi¹zana ze zg³oszeniem (relacja nawigacyjna).
        /// </summary>
        public Visit Visit { get; set; } = default!;

        /// <summary>
        /// Historia zmian zg³oszenia (snapshoty).
        /// </summary>
        public ICollection<SubmissionSnapshot> History { get; set; } = new List<SubmissionSnapshot>();
    }
}