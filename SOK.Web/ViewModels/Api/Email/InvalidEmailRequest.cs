namespace SOK.Web.ViewModels.Api.Email
{
    public class InvalidEmailRequest : EmailRequest
    {
        public string To { get; set; } = string.Empty;
    }
}
