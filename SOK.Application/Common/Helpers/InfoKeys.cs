namespace SOK.Application.Common.Helpers
{
    public static class InfoKeys
    {
        public static class Parish
        {
            public const string FullName = "Parish.FullName";
            public const string ShortName = "Parish.ShortName";
            public const string ShortNameAppendix = "Parish.ShortNameAppendix";
            public const string UniqueId = "Parish.UniqueId";
            public const string Diocese = "Parish.Diocese";
        }

        public static class Contact
        {
            public const string Email = "Contact.Email";
            public const string MainPhone = "Contact.MainPhone";
            public const string SecondaryPhone = "Contact.SecondaryPhone";
            public const string Website = "Contact.Website";
            public const string StreetAndBuilding = "Contact.StreetAndBuilding";
            public const string CityName = "Contact.CityName";
            public const string PostalCode = "Contact.PostalCode";
            public const string RegionAndCountry = "Contact.RegionAndCountry";
        }

        public static class Email
        {
            public const string EnableEmailSending = "Email.EnableEmailSending";
            public const string SmtpServer = "Email.SmtpServer";
            public const string SmtpRequireAuth = "Email.SmtpRequireAuth";
            public const string SmtpPort = "Email.SmtpPort";
            public const string SmtpUserName = "Email.SmtpUserName";
            public const string SmtpPassword = "Email.SmtpPassword";
            public const string SmtpEnableSsl = "Email.SmtpEnableSsl";
            public const string SenderEmail = "Email.SenderEmail";
            public const string SenderName = "Email.SenderName";
            public const string BccRecipients = "Email.BccRecipients";
        }

        public static class EmbeddedApplication
        {
            public const string FormUrl = "Application.FormUrl";
            public const string ControlPanelBaseUrl = "Application.ControlPanelBaseUrl";
        }
    }
}