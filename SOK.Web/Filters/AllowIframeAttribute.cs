using Microsoft.AspNetCore.Mvc.Filters;

namespace SOK.Web.Filters
{
    /// <summary>
    /// Atrybut pozwalający na osadzenie strony w iframe
    /// </summary>
    public class AllowIframeAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            // Usuń nagłówek X-Frame-Options jeśli istnieje
            context.HttpContext.Response.Headers.Remove("X-Frame-Options");
            
            // Ustaw Content-Security-Policy aby pozwolić na osadzanie w iframe
            // frame-ancestors 'self' * pozwala na osadzanie z dowolnej domeny
            context.HttpContext.Response.Headers.Append("Content-Security-Policy", "frame-ancestors *");
            
            base.OnResultExecuting(context);
        }
    }
}
