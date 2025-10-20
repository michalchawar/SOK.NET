using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;

namespace SOK.Application.Common.DTO
{
    public class SubmissionCreationRequestDto
    {
        public Submitter Submitter { get; set; }
        public Schedule Schedule { get; set; }

        public string SubmitterNotes { get; set; } = string.Empty;

        public Building Building { get; set; }
        public int? ApartmentNumber { get; set; } = null;
        public string? ApartmentLetter { get; set; } = string.Empty;

        public DateTime Created { get; set; } = DateTime.Now;
        public User Author { get; set; }
        public SubmitMethod Method { get; set; } = SubmitMethod.NotRegistered;
        public string IPAddress { get; set; } = string.Empty;
    }
}
