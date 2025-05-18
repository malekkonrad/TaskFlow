using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;



public class SessionAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        if (session.GetString("Username") == null)
        {
            context.Result = new RedirectToRouteResult(
                new RouteValueDictionary {
                    { "Controller", "Auth" },
                    { "Action", "Login" }
                });
        }
    }
}
