namespace SOK.Web.ViewModels.Api.Settings
{
    public class UpdateSettingsRequest
    {
        public List<UpdateSettingRequest> Settings { get; set; } = new();
    }
}
