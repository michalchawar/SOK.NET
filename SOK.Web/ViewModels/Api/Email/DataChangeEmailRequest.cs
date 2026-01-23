using SOK.Application.Common.Helpers.EmailTypes;

namespace SOK.Web.ViewModels.Api.Email
{
    public class DataChangeEmailRequest : EmailRequest
    {
        public DataChanges Changes { get; set; } = new();
    }
}
