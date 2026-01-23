using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SOK.Application.Services.Interface;

namespace SOK.Web.Filters
{
    /// <summary>
    /// Atrybut filtra sprawdzający czy w parafii jest ustawiony aktywny plan.
    /// Jeśli nie ma aktywnego planu, użytkownik zostanie przekierowany do widoku z informacją.
    /// </summary>
    /// <remarks>
    /// Wymagany jest aktywny plan, aby móc pracować z harmonogramami i zgłoszeniami.
    /// </remarks>
    public class RequireActivePlanAttribute : TypeFilterAttribute
    {
        public RequireActivePlanAttribute() : base(typeof(RequireActivePlanFilter))
        {
        }

        private class RequireActivePlanFilter : IAsyncActionFilter
        {
            private readonly IPlanService _planService;

            public RequireActivePlanFilter(IPlanService planService)
            {
                _planService = planService;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var activePlan = await _planService.GetActivePlanAsync();

                if (activePlan == null)
                {
                    // Przekieruj do widoku z informacją o braku aktywnego planu
                    context.Result = new ViewResult
                    {
                        ViewName = "~/Views/Shared/NoActivePlan.cshtml"
                    };
                    return;
                }

                await next();
            }
        }
    }
}
