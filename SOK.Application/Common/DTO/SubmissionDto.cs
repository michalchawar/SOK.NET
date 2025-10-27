using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;

namespace SOK.Application.Common.DTO
{
    public class SubmissionDto
    {
        public int Id { get; private set; }
        public string UniqueId { get; private set; } = string.Empty;

        public SubmitterDto Submitter { get; set; }
        public AddressDto Address { get; set; }

        public string SubmitterNotes { get; set; } = string.Empty;
        public string AdminMessage { get; set; } = string.Empty;
        public string AdminNotes { get; set; } = string.Empty;
        public NotesFulfillmentStatus NotesStatus { get; set; }
        public DateTime SubmitTime { get; set; }

        public SubmissionDto(Submission submission)
        {
            Id = submission.Id;
            UniqueId = submission.UniqueId.ToString();
            SubmitterNotes = submission.SubmitterNotes ?? string.Empty;
            AdminMessage = submission.AdminMessage ?? string.Empty;
            AdminNotes = submission.AdminNotes ?? string.Empty;
            NotesStatus = submission.NotesStatus;
            SubmitTime = submission.SubmitTime;

            Submitter = new SubmitterDto(submission.Submitter);
            Address = new AddressDto(submission.Address);
        }
    }
}
