using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Helpers.EmailTypes
{
    /// <summary>
    /// Abstrakcyjna klasa bazowa dla wszystkich typów emaili
    /// </summary>
    public abstract class EmailTypeBase
    {
        /// <summary>
        /// Nazwa szablonu (bez rozszerzenia .html)
        /// </summary>
        public abstract string TemplateName { get; }

        /// <summary>
        /// Domyślny temat emaila
        /// </summary>
        public abstract string DefaultSubject { get; }

        /// <summary>
        /// Adresat emaila
        /// </summary>
        public abstract string To { get; }

        /// <summary>
        /// Priorytet wysyłki (0-10)
        /// </summary>
        public virtual int Priority => 5;

        /// <summary>
        /// Buduje słownik z danymi do podstawienia w szablonie
        /// </summary>
        /// <returns>Słownik placeholder -> wartość</returns>
        public abstract Dictionary<string, string> BuildTemplateData();

        /// <summary>
        /// Opcjonalnie można nadpisać temat
        /// </summary>
        public string? CustomSubject { get; set; }

        /// <summary>
        /// Pobiera temat emaila (custom lub domyślny)
        /// </summary>
        public string GetSubject() => CustomSubject ?? DefaultSubject;
    }
}
