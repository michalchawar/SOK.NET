using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SOK.Web.Filters
{
    /// <summary>
    /// Atrybut filtra sprawdzający czy użytkownik ma dostęp do kontekstu parafialnego.
    /// Dla zwykłych użytkowników wymaga przypisanej parafii.
    /// Dla SuperAdmin wymaga wybranej parafii przez ciasteczko.
    /// </summary>
    /// <remarks>
    /// Jeśli SuperAdmin nie ma wybranej parafii, zostanie przekierowany do ParishManagement.
    /// Jeśli zwykły użytkownik nie ma przypisanej parafii, wyświetlony zostanie błąd.
    /// </remarks>
    public class RequireParishAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Wykonuje się przed akcją kontrolera.
        /// Sprawdza czy użytkownik ma dostęp do kontekstu parafialnego.
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var isSuperAdmin = user.IsInRole("SuperAdmin");
            
            if (isSuperAdmin)
            {
                // Sprawdź czy ParishResolver wykrył nieprawidłowe ciasteczko
                if (context.HttpContext.Items.ContainsKey("InvalidParishCookie"))
                {
                    // Przekieruj do wyboru parafii
                    context.Result = new RedirectToActionResult(
                        "Index", 
                        "ParishManagement", 
                        null);
                    return;
                }
                
                // SuperAdmin musi mieć wybrane ciasteczko z parafią
                if (!context.HttpContext.Request.Cookies.ContainsKey("SelectedParishUid"))
                {
                    // Przekieruj do wyboru parafii
                    context.Result = new RedirectToActionResult(
                        "Index", 
                        "ParishManagement", 
                        null);
                    return;
                }
            }
            // Dla zwykłych użytkowników sprawdzenie odbywa się w middleware ParishResolver
            // który korzysta z przypisanej parafii w bazie danych
            
            base.OnActionExecuting(context);
        }
    }
}
