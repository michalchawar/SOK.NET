using SOK.Application.Services.Interface;

namespace SOK.Web.Middleware
{
    /// <summary>
    /// Middleware odpowiedzialne za ustawienie odpowiedniej parafii (tenant'a).
    /// </summary>
    public class ParishResolver : IMiddleware
    {
        private readonly ICurrentParishService _currentParishService;
        //private readonly ILogger<ParishResolver> _logger;

        public ParishResolver(ICurrentParishService currentParishService)
        {
            _currentParishService = currentParishService;
        }

        /// <summary>
        /// Przetwarza żądanie HTTP, ustawiając odpowiednią parafię (tenant'a)
        /// na podstawie użytkownika, ustawionego przez middleware uwierzytelniające,
        /// lub na podstawie parametru routingu (dla anonimowych użytkowników).
        /// </summary>
        /// <param name="context">Kontekst HTTP.</param>
        /// <param name="next">Następny element w potoku przetwarzania żądań.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący operację asynchroniczną.
        /// </returns>
        /// <remarks>
        /// Metoda pobiera identyfikator parafii (parishUid) z claimów użytkownika
        /// lub z parametru routingu "parishUid" dla publicznych formularzy.
        /// </remarks>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string? parishUid;
            
            // Najpierw sprawdź parametr routingu (dla anonimowych użytkowników)
            if (context.Request.RouteValues.TryGetValue("parishUid", out var routeParishUid))
            {
                parishUid = routeParishUid?.ToString();
            }
            // Jeśli nie ma w routingu, sprawdź claims użytkownika (dla zalogowanych)
            else
            {
                parishUid = context.User.FindFirst("ParishUniqueId")?.Value;
            }

            if (!string.IsNullOrEmpty(parishUid))
            {
                try
                {
                    var done = await _currentParishService.SetParishAsync(parishUid);
                    if (done)
                        Console.WriteLine($"ParishResolver: Using the parish with UID: {parishUid}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"ParishResolver: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("ParishResolver: No parish identifier found in the request.");
            }

            await next(context);
        }
    }
}
