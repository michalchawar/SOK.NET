using Microsoft.Extensions.Options;
using app.Services.ParishService;

namespace app.Middleware
{
    /// <summary>
    /// Middleware odpowiedzialne za ustawienie odpowiedniej parafii (tenant'a) na podstawie ciasteczka
    /// </summary>
    public class ParishResolver : IMiddleware
    {
        private readonly ICurrentParishService _currentParishService;
        private readonly ILogger<ParishResolver> _logger;

        public ParishResolver(ICurrentParishService currentParishService, ILogger<ParishResolver> logger)
        {
            _currentParishService = currentParishService;
            _logger = logger;
        }

        /// <summary>
        /// Przetwarza żądanie HTTP, ustawiając odpowiednią parafię (tenant'a) na podstawie ciasteczka
        /// </summary>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Cookies.TryGetValue("parishUid", out var cookieValue))
            {
                var done = await _currentParishService.SetParish(cookieValue);

                if (done)
                    _logger.LogInformation($"Using the parish with UID: {cookieValue}");
            }
            else
            {
                _logger.LogInformation("No parish cookie found in the request.");
            }

            await next(context);
        }
    }
}
