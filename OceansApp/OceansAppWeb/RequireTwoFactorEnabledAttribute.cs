using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

public class RequireTwoFactorEnabledAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.User.Identity.IsAuthenticated)
        {
            var userManager = context.HttpContext.RequestServices.GetService<UserManager<IdentityUser>>();
            var user = await userManager.GetUserAsync(context.HttpContext.User);

            if (user != null && !await userManager.GetTwoFactorEnabledAsync(user))
            {
                context.Result = new RedirectToActionResult("EnableAuthenticator", "Account", new { area = "" });
                return;
            }
        }

        await base.OnActionExecutionAsync(context, next);
    }
}
