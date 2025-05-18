using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AdminAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var role = context.HttpContext.Session.GetString("Role");
        var isAuthenticated = !string.IsNullOrEmpty(role); // Użytkownik jest zalogowany

        if (role != "ADMIN")
        {
            if (isAuthenticated)
            {
                // Użytkownik jest zalogowany, ale nie ma uprawnień administratora
                context.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "Controller", "Home" },  // lub inna strona z komunikatem
                    { "Action", "AccessDenied" }
                });
                
                // Możesz też dodać komunikat
                context.HttpContext.Session.SetString("AccessDeniedMessage", 
                    "Nie masz wystarczających uprawnień do wykonania tej operacji.");
            }
            else
            {
                // Użytkownik nie jest zalogowany
                context.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "Controller", "Auth" },
                    { "Action", "Login" }
                });
            }
        }
    }
}
