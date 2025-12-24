using SOK.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje zgłoszenie złożone przez prywatną osobę zgłaszającą 
    /// lub zalogowanego użytkownika w jej imieniu. Zawiera dane powiązane z formularzem, 
    /// adres, wszelakie uwagi i ich status, powiązania z planem, wizytą i historią zmian.
    /// </summary>
    public class Submission
    {
        /// <summary>
        /// Unikalny identyfikator zgłoszenia (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator zgłoszenia (GUID), wykorzystywany głównie do udostępniania lub autoryzacji (w połączeniu z AccessToken).
        /// </summary>
        public Guid UniqueId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Token dostępu do zgłoszenia, używany do autoryzacji w połączeniu z UniqueId.
        /// </summary>
        [MaxLength(64)]
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// Uwagi zgłaszającego (opcjonalne).
        /// </summary>
        [MaxLength(512)]
        public string? SubmitterNotes { get; set; } = null;

        /// <summary>
        /// Wiadomość administracyjna skierowana do zgłaszającego (opcjonalna).
        /// </summary>
        [MaxLength(512)]
        public string? AdminMessage { get; set; } = null;

        /// <summary>
        /// Systemowe notatki administratora dotyczące zgłoszenia (opcjonalne).
        /// </summary>
        [MaxLength(512)]
        public string? AdminNotes { get; set; } = null;

        /// <summary>
        /// Status realizacji uwag od użytkownika.
        /// Status jest równy NotesFulfillmentStatus.NA, jeśli zgłoszenie nie zawiera uwag od użytkownika.
        /// </summary>
        [DefaultValue(NotesFulfillmentStatus.NA)]
        public NotesFulfillmentStatus NotesStatus { get; set; } = NotesFulfillmentStatus.NA;

        /// <summary>
        /// Data i godzina rejestracji zgłoszenia.
        /// </summary>
        public DateTime SubmitTime { get; private set; }

        /// <summary>
        /// Identyfikator osoby zgłaszającej (Submitter).
        /// </summary>
        public int SubmitterId { get; set; }

        /// <summary>
        /// Osoba zgłaszająca (relacja nawigacyjna).
        /// </summary>
        public Submitter Submitter { get; set; } = default!;

        /// <summary>
        /// Identyfikator adresu powiązanego ze zgłoszeniem.
        /// </summary>
        public int AddressId { get; set; }

        /// <summary>
        /// Adres powiązany ze zgłoszeniem (relacja nawigacyjna).
        /// </summary>
        public Address Address { get; set; } = default!;

        /// <summary>
        /// Powiązane archiwalne dane formularza zgłoszeniowego.
        /// </summary>
        public FormSubmission FormSubmission { get; set; } = default!;

        /// <summary>
        /// Wizyta powiązana ze zgłoszeniem (relacja nawigacyjna).
        /// </summary>
        public Visit Visit { get; set; } = default!;

        /// <summary>
        /// Identyfikator planu, do którego należy zgłoszenie.
        /// </summary>
        /// <remarks>
        /// Ta właściwość jest ustawiana podczas tworzenia zgłoszenia i nie może być modyfikowana później.
        /// </remarks>
        public int PlanId { get; private set; }
        
        /// <summary>
        /// Plan, do którego należy zgłoszenie (relacja nawigacyjna).
        /// </summary>
        /// <remarks>
        /// Ta właściwość jest ustawiana podczas tworzenia zgłoszenia i nie może być modyfikowana później.
        /// </remarks>
        public Plan Plan { get; private set; } = default!;

        /// <summary>
        /// Historia zmian zgłoszenia (snapshoty).
        /// </summary>
        public ICollection<SubmissionSnapshot> History { get; set; } = new List<SubmissionSnapshot>();
        
        /// <summary>
        /// Wysłane (bądź zakolejkowane) maile.
        /// </summary>
        public ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();

        /// <summary>
        /// Konstruktor bezparametrowy.
        /// </summary>
        public Submission() {}

        /// <summary>
        /// Konstruktor tworzący zgłoszenie powiązane z określonym planem.
        /// </summary>
        /// <param name="plan">Plan, do którego należy zgłoszenie.</param>
        public Submission(Plan plan, DateTime? submitTime = null)
        {
            PlanId = plan.Id;
            Plan = plan;

            if (submitTime.HasValue)
                SubmitTime = submitTime.Value;
        }

        /// <summary>
        /// Konstruktor tworzący zgłoszenie powiązane z planem przez ID.
        /// </summary>
        /// <param name="planId">ID planu, do którego należy zgłoszenie.</param>
        public Submission(int planId, DateTime? submitTime = null)
        {
            PlanId = planId;

            if (submitTime.HasValue)
                SubmitTime = submitTime.Value;
        }
    }
}