using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Web.Middleware
{
    /// <summary>
    /// Middleware odpowiedzialne za ustawienie odpowiedniej parafii (tenant'a).
    /// </summary>
    public class ParishResolver : IMiddleware
    {
        private readonly ICurrentParishService _currentParishService;
        private readonly UserManager<User> _userManager;
        private readonly CentralDbContext _centralDb;
        private readonly ILogger<ParishResolver> _logger;

        public ParishResolver(ICurrentParishService currentParishService, UserManager<User> userManager, CentralDbContext centralDb, ILogger<ParishResolver> logger)
        {
            _currentParishService = currentParishService;
            _userManager = userManager;
            _centralDb = centralDb;
            _logger = logger;
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
        /// 
        /// Logika wyboru parafii:
        /// 1. Dla niezalogowanych użytkowników: próba odczytu z parametru routingu
        /// 2. Dla zalogowanych użytkowników:
        ///    a) SuperAdmin: odczyt z ciasteczka "SelectedParishUid" (jeśli istnieje)
        ///    b) Zwykły użytkownik: odczyt z przypisanej parafii w bazie centralnej
        /// </remarks>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string? parishUid = null;
            bool isSuperAdmin = false;
            
            // Sprawdź parametr routingu
            if (context.Request.RouteValues.TryGetValue("parishUid", out var routeParishUid))
            {
                parishUid = routeParishUid?.ToString();
            }
            // Dla zalogowanych użytkowników sprawdź przypisaną parafię
            else if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(context.User);

                if (!string.IsNullOrEmpty(userId))
                {
                    // Pobierz użytkownika z załadowaną relacją Parish
                    var user = await _centralDb.Users
                        .Include(u => u.Parish)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    if (user != null)
                    {
                        isSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");
                        
                        if (isSuperAdmin)
                        {
                            // SuperAdmin: sprawdź ciasteczko z wybraną parafią
                            if (context.Request.Cookies.TryGetValue("SelectedParishUid", out var selectedParishUid))
                            {
                                parishUid = selectedParishUid;
                            }
                            // Jeśli nie ma ciasteczka, SuperAdmin może działać bez wybranej parafii
                        }
                        else
                        {
                            // Zwykły użytkownik: użyj przypisanej parafii
                            if (user.Parish != null)
                            {
                                parishUid = user.Parish.UniqueId.ToString();
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(parishUid))
            {
                try
                {
                    var done = await _currentParishService.SetParishAsync(parishUid);
                    if (done)
                        _logger.LogInformation("ParishResolver: Using the parish with UID: {ParishUid}", parishUid);

                    else if (isSuperAdmin)
                    {
                        _logger.LogWarning("ParishResolver: SuperAdmin with invalid parish UID: {ParishUid}", parishUid);
                        context.Response.Cookies.Delete("SelectedParishUid");
                        // Ustaw flagę, że ciasteczko było nieprawidłowe
                        context.Items["InvalidParishCookie"] = true;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "ParishResolver: {ErrorMessage}", ex.Message);
                }
            }
            else
            {
                _logger.LogInformation("ParishResolver: No parish identifier found in the request");
            }

            await next(context);
        }
    }
}
