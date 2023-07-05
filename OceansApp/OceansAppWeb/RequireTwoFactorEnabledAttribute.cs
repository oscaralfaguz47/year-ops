using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RequireTwoFactorEnabledAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.User.Identity.IsAuthenticated)
        {
            var userManager = context.HttpContext.RequestServices.GetService<UserManager<IdentityUser>>();
            var signInManager = context.HttpContext.RequestServices.GetService<SignInManager<IdentityUser>>();
            var user = await userManager.GetUserAsync(context.HttpContext.User);

            if (user != null && (!await userManager.GetTwoFactorEnabledAsync(user) || user.LockoutEnd > DateTime.Now))
            {
                await userManager.ResetAuthenticatorKeyAsync(user);
                await userManager.SetTwoFactorEnabledAsync(user, false);
                await signInManager.SignOutAsync();
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                return;
            }
        }

        await base.OnActionExecutionAsync(context, next);
    }
}
