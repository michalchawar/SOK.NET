namespace SOK.Application.Common.Helpers.EmailTypes
{
    /// <summary>
    /// Email z niestandardową treścią (blank template)
    /// </summary>
    public class CustomEmail : EmailTypeBase
    {
        private readonly Dictionary<string, string> _customData;

        public override string TemplateName => "blank";
        public override string DefaultSubject => "Wiadomość";
        public override string To => string.Empty;
        public override int Priority => 5;

        /// <summary>
        /// Tworzy email z niestandardową treścią
        /// </summary>
        /// <param name="content">Treść wiadomości (HTML)</param>
        /// <param name="subject">Temat emaila</param>
        public CustomEmail(string content, string? subject = null)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content cannot be empty", nameof(content));

            _customData = new Dictionary<string, string>
            {
                ["content"] = content
            };

            if (!string.IsNullOrWhiteSpace(subject))
                CustomSubject = subject;
        }

        /// <summary>
        /// Tworzy email z dowolnymi danymi do podstawienia
        /// </summary>
        /// <param name="templateData">Słownik z danymi</param>
        /// <param name="subject">Temat emaila</param>
        public CustomEmail(Dictionary<string, string> templateData, string? subject = null)
        {
            _customData = templateData ?? throw new ArgumentNullException(nameof(templateData));

            if (!string.IsNullOrWhiteSpace(subject))
                CustomSubject = subject;
        }

        public override Dictionary<string, string> BuildTemplateData()
        {
            return new Dictionary<string, string>(_customData);
        }
    }
}
