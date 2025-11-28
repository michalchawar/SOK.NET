using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;

namespace SOK.Application.Common.DTO
{
    public class SubmissionCreationRequestDto
    {
        public Submitter Submitter { get; set; } = default!;
        public Schedule Schedule { get; set; } = default!;

        public string? SubmitterNotes { get; set; } = null;
        public string? AdminNotes { get; set; } = null;

        public Building Building { get; set; } = default!;
        public int? ApartmentNumber { get; set; } = null;
        public string? ApartmentLetter { get; set; } = null;

        public DateTime Created { get; set; } = DateTime.Now;
        public ParishMember? Author { get; set; } = null;
        public SubmitMethod Method { get; set; } = SubmitMethod.NotRegistered;
        public string? IPAddress { get; set; } = null;

        public bool SendConfirmationEmail { get; set; } = false;
    }
}
