using System.Text.RegularExpressions;

namespace SOK.Application.Common.Helpers
{
    /// <summary>
    /// Serwis do ładowania i przetwarzania szablonów email
    /// </summary>
    public class EmailTemplateService
    {
        private readonly string _templatesPath;

        public EmailTemplateService()
        {
            // Ścieżka do katalogu z szablonami
            _templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Email");
        }

        /// <summary>
        /// Ładuje szablon email z dysku
        /// </summary>
        /// <param name="templateName">Nazwa szablonu (bez rozszerzenia .html)</param>
        /// <returns>Zawartość szablonu</returns>
        public async Task<string> LoadTemplateAsync(string templateName)
        {
            string templatePath = Path.Combine(_templatesPath, $"{templateName}.html");
            
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Szablon email '{templateName}' nie został znaleziony.", templatePath);
            }

            return await File.ReadAllTextAsync(templatePath);
        }

        /// <summary>
        /// Buduje kompletny email z header, body i footer
        /// </summary>
        /// <param name="templateName">Nazwa szablonu body (np. "confirmation")</param>
        /// <param name="data">Słownik z danymi do podstawienia</param>
        /// <returns>Kompletna treść HTML emaila</returns>
        public async Task<string> BuildEmailAsync(string templateName, Dictionary<string, string> data)
        {
            // Ładujemy header, body i footer
            string header = await LoadTemplateAsync("header");
            string body = await LoadTemplateAsync(templateName);
            string footer = await LoadTemplateAsync("footer");

            // Łączymy wszystko
            string fullTemplate = header + body + footer;

            // Podstawiamy dane
            return ReplacePlaceholders(fullTemplate, data);
        }

        /// <summary>
        /// Zastępuje placeholdery (np. %submitter_name%) wartościami z dictionary
        /// </summary>
        /// <param name="template">Szablon z placeholderami</param>
        /// <param name="data">Dane do podstawienia</param>
        /// <returns>Szablon z podstawionymi danymi</returns>
        public string ReplacePlaceholders(string template, Dictionary<string, string> data)
        {
            if (data == null || data.Count == 0)
                return template;

            foreach (var kvp in data)
            {
                string placeholder = $"%{kvp.Key}%";
                template = template.Replace(placeholder, kvp.Value ?? string.Empty);
            }

            return template;
        }

        /// <summary>
        /// Sprawdza, czy szablon istnieje
        /// </summary>
        /// <param name="templateName">Nazwa szablonu</param>
        /// <returns>True jeśli szablon istnieje</returns>
        public bool TemplateExists(string templateName)
        {
            string templatePath = Path.Combine(_templatesPath, $"{templateName}.html");
            return File.Exists(templatePath);
        }

        /// <summary>
        /// Pobiera listę dostępnych szablonów
        /// </summary>
        /// <returns>Lista nazw szablonów (bez rozszerzenia)</returns>
        public List<string> GetAvailableTemplates()
        {
            if (!Directory.Exists(_templatesPath))
                return new List<string>();

            return Directory.GetFiles(_templatesPath, "*.html")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => name != "header" && name != "footer")
                .ToList()!;
        }

        /// <summary>
        /// Wyciąga wszystkie placeholdery z szablonu
        /// </summary>
        /// <param name="templateName">Nazwa szablonu</param>
        /// <returns>Lista znalezionych placeholderów</returns>
        public async Task<List<string>> GetTemplatePlaceholdersAsync(string templateName)
        {
            string template = await LoadTemplateAsync(templateName);
            var matches = Regex.Matches(template, @"%(\w+)%");
            
            return matches
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .ToList();
        }
    }
}
