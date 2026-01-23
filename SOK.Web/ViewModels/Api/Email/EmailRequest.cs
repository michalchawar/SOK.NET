namespace SOK.Web.ViewModels.Api.Email
{
    public abstract class EmailRequest
    {
        public int SubmissionId { get; set; }
        public int Priority { get; set; } = 5;
        public bool ForceSend { get; set; } = true;
    }
}
