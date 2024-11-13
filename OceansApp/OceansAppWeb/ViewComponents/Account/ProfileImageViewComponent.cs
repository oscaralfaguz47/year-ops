using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace OceansAppWeb.ViewComponents.Account
{
    public class ProfileImageViewComponent : ViewComponent
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileImageViewComponent(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            string profileImageUrl = null;

            if (user != null)
            {
                var claimsPrincipal = (ClaimsPrincipal)HttpContext.User;
                profileImageUrl = claimsPrincipal.Claims
                    .FirstOrDefault(c => c.Type == "ProfileImageUrl")?.Value;
            }

            return View("Default", profileImageUrl);
        }
    }

}
