using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

public abstract class BaseController : Controller
{
    // BaseController.cs
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        if (User?.Identity?.IsAuthenticated == true)
        {
            ViewData["Layout"] = User.IsInRole("Admin") ? "_AdminLayout" : "_Layout";
        }
        else
        {
            ViewData["Layout"] = "_Layout";
        }
    }
}
