using Microsoft.Extensions.Options;
using SOK.Web.Services.Parish;

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
            //_logger = logger;
        }

        /// <summary>
        /// Przetwarza żądanie HTTP, ustawiając odpowiednią parafię (tenant'a)
        /// na podstawie użytkownika, ustawionego przez middleware uwierzytelniające.
        /// </summary>
        /// <param name="context">Kontekst HTTP.</param>
        /// <param name="next">Następny element w potoku przetwarzania żądań.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/> reprezentujący operację asynchroniczną.
        /// </returns>
        /// <remarks>
        /// Metoda pobiera identyfikator parafii (parishUid) z claimów użytkownika,
        /// powinny one być ustawione przez middleware uwierzytelniające, domyślnie jest to Identity.
        /// </remarks>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var parishUid = context.User.FindFirst("ParishUniqueId")?.Value;
            if (!string.IsNullOrEmpty(parishUid))
            {
                var done = await _currentParishService.SetParish(parishUid);
                if (done)
                    Console.WriteLine($"ParishResolver: Using the parish with UID: {parishUid}");
                    //_logger.LogInformation($"Using the parish with UID: {parishUid}");
            }
            else
            {
                Console.WriteLine("ParishResolver: No parish cookie found in the request.");
                //_logger.LogInformation("No parish cookie found in the request.");
            }

            await next(context);
        }
    }
}
