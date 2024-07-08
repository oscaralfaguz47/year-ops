using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

public class RequireTwoFactorEnabledAttribute : ActionFilterAttribute
{
    private readonly IMemoryCache _cache;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public RequireTwoFactorEnabledAttribute(
        IMemoryCache cache,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
    {
        _cache = cache;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.User.Identity.IsAuthenticated)
        {
            context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
            return;
        }

        var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId != null)
        {
            // Check if the UserSessionChangesExpiration variable has expired
            var cacheKey = $"UserSessionChangesExpiration_{userId}";
            if (!_cache.TryGetValue(cacheKey, out DateTimeOffset expiration) || expiration <= DateTimeOffset.Now)
            {
                // Si la caché no existe o ha expirado, invalidar la sesión y redirigir al login o a SessionEnded
                await InvalidateUserSession(userId);

                if (expiration <= DateTimeOffset.Now)
                {
                    context.Result = new RedirectResult("/SessionEnded");
                }
                else
                {
                    context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                }
                return;
            }

            var twoFactorEnabledClaim = context.HttpContext.User.FindFirst("amr");
            var twoFactorRequiredClaim = context.HttpContext.User.FindFirst("TwoFactorRequired");

            var twoFactorEnabled = twoFactorEnabledClaim != null && twoFactorEnabledClaim.Value == "mfa";
            var twoFactorRequired = twoFactorRequiredClaim != null && bool.Parse(twoFactorRequiredClaim.Value);

            if (!twoFactorEnabled && twoFactorRequired)
            {
                await InvalidateUserSession(userId);
                context.Result = new RedirectResult("/SessionEnded?reason=two_factor");
                return;
            }

            var roleClaimsInIdentity = context.HttpContext.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint != null)
            {
                var requiredRoleClaims = endpoint.Metadata.GetMetadata<AuthorizeAttribute>()?.Roles?.Split(',');
                if (requiredRoleClaims != null && !requiredRoleClaims.All(role => roleClaimsInIdentity.Contains(role)))
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Home", new { area = "" });
                    return;
                }

            }
        }

        await base.OnActionExecutionAsync(context, next);
    }

    private async Task InvalidateUserSession(string userId)
    {
        var cacheKey = $"UserSessionChangesExpiration_{userId}";
        _cache.Remove(cacheKey);
        await _signInManager.SignOutAsync();
    }
}
