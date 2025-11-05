using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SOK.Web.Filters
{
    /// <summary>
    /// Ustawia ViewData["ActivePage"], aby umożliwić podświetlanie aktywnej zakładki w widoku.
    /// Może być stosowany zarówno na kontrolerze, jak i na konkretnych akcjach.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ActivePageAttribute : ActionFilterAttribute
    {
        private readonly string _pageName;

        public ActivePageAttribute(string pageName)
        {
            _pageName = pageName;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                controller.ViewData["ActivePage"] = _pageName;
            }

            base.OnActionExecuting(context);
        }
    }
}