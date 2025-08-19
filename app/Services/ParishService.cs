using Microsoft.Extensions.Options;

namespace app.Services
{
    /// <summary>
    /// Reprezentuje konfigurację pojedynczej parafii (tenant'a)
    /// </summary>
    public class ParishConfiguration
    {
        public string UniqueId { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sekcja konfiguracyjna zawierająca listę parafii (tenant'ów)
    /// </summary>
    public class ParishConfigurationSection
    {
        public List<ParishConfiguration> Parishes { get; set; } = new();
    }

    /// <summary>
    /// Serwis zarządzający aktualną parafią (tenant'em)
    /// </summary>
    public class ParishService : IParishGetter, IParishSetter
    {
        public ParishConfiguration Parish { get; private set; } = default!;

        public void SetParish(ParishConfiguration parish)
        {
            Parish = parish;
        }
    }

    /// <summary>
    /// Middleware odpowiedzialne za ustawienie odpowiedniej parafii (tenant'a) na podstawie ciasteczka
    /// </summary>
    public class MultiParishServiceMiddleware : IMiddleware
    {
        private readonly IParishSetter setter;
        private readonly IOptions<ParishConfigurationSection> config;
        private readonly ILogger<MultiParishServiceMiddleware> logger;

        public MultiParishServiceMiddleware(
            IParishSetter setter,
            IOptions<ParishConfigurationSection> config,
            ILogger<MultiParishServiceMiddleware> logger)
        {
            this.setter = setter;
            this.config = config;
            this.logger = logger;
        }

        /// <summary>
        /// Przetwarza żądanie HTTP, ustawiając odpowiednią parafię (tenant'a) na podstawie ciasteczka
        /// </summary>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var parish = config.Value.Parishes.First();

            if (context.Request.Cookies.TryGetValue("tenant", out var cookieValue))
            {
                var key = cookieValue;
                parish = config.Value
                    .Parishes
                    .FirstOrDefault(t => t.UniqueId.Equals(key?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? parish;
            }

            logger.LogInformation("Using the parish with UID: {parish}", parish.UniqueId);
            setter.SetParish(parish);

            await next(context);
        }
    }

    /// <summary>
    /// Interfejs do pobierania informacji o aktualnej parafii (tenant'cie)
    /// </summary>
    public interface IParishGetter
    {
        ParishConfiguration Parish { get; }
    }

    /// <summary>
    /// Interfejs do ustawiania aktualnej parafii (tenant'a)
    /// </summary>
    public interface IParishSetter
    {
        void SetParish(ParishConfiguration parish);
    }
}
