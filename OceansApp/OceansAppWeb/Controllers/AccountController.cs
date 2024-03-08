using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels;
using OceansApp.Utility;
using OceansApp.Utility.LazyLoading;
using OceansApp.Utility.NotificationTemplates;
using OceansApp.Utility.SharedMethods;
using OceansAppWeb.Controllers;
using System.Text.Encodings.Web;

namespace OceansAppWeb.Account.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UrlEncoder _urlEncoder;
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly LazyServiceProvider<ISendEmailRepository> _sendEmailRepository;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager
            , UrlEncoder urlEncoder, ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOrWork,
            IHttpContextAccessor httpContextAccessor, IConfiguration config, LazyServiceProvider<ISendEmailRepository> sendEmailRepository,
            IBackgroundTaskQueue backgroundTaskQueue)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
            _dbContext = dbContext;
            _roleManager = roleManager;
            _unitOfWork = unitOrWork;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _sendEmailRepository = sendEmailRepository;
            _backgroundTaskQueue = backgroundTaskQueue;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        [RequireTwoFactorEnabled]
        public async Task<IActionResult> ProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var userFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == user.Id);

            ProfileVM myInfo = new()
            {
                Id = userFromDb.Id,
                Email = userFromDb.Email,
                Name = userFromDb.Name,
                LastName = userFromDb.LastName,
                Ocupation = userFromDb.Occupation,
                PhoneNumber = userFromDb.PhoneNumber
            };
            ViewData["Title"] = "My Account Settings";
            return View(myInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userToUpdate = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == model.Id);

                    userToUpdate.Name = model.Name;
                    userToUpdate.LastName = model.LastName;
                    userToUpdate.Occupation = model.Ocupation;
                    userToUpdate.PhoneNumber = model.PhoneNumber;

                    _unitOfWork.Save();

                    TempData["success"] = "Data was saved successfully!";
                    return View("Profile", model);
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            return View("Profile", model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                var resetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
                return RedirectToAction("CreatePassword", "Account", new { code = resetCode, email = user.Email });
            }
            else
            {
                return View("Error");
            }

        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(HomeController.Dashboard), "Home");
            }
            else
            {
                if (string.IsNullOrEmpty(returnUrl))
                {
                    returnUrl = Url.Content("~/");
                }
                ViewData["Title"] = "Inicio de sesión";
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    if (user != null && user.EmailConfirmed)
                    {
                        var applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == user.Id);
                        if (applicationUser.IsActive)
                        {
                            if ((DateTime.UtcNow > applicationUser.OpaqueTokenExpiration)
                                || (applicationUser.OpaqueToken == null && applicationUser.OpaqueTokenExpiration == null))
                            {
                                GenerateTokensAndRandomStrings sharedMethod = new();
                                string newToken = sharedMethod.GenerateOpaqueToken();

                                applicationUser.OpaqueToken = newToken;
                                applicationUser.OpaqueTokenExpiration = DateTime.UtcNow.AddMinutes(SD.OpaqueTokenExpirationTime);

                                await _userManager.UpdateAsync(user);
                            }
                            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                            if (result.Succeeded)
                            {
                                if (!user.TwoFactorEnabled)
                                {
                                    return RedirectToAction("EnableAuthenticator");
                                }
                                return LocalRedirect(returnUrl);
                            }

                            if (result.RequiresTwoFactor)
                            {
                                return RedirectToAction(nameof(VerifyAuthenticatorCode), new { returnUrl, model.RememberMe });
                            }

                            if (result.IsLockedOut)
                            {
                                return View("Lockout");
                            }
                        }
                        else
                        {
                            // Consider logging the login attempt of an inactive account
                        }
                    }
                    ModelState.AddModelError(string.Empty, "Your credentials are incorrect.");
                }
                catch (Exception e)
                {
                    return View("Error");
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EnableAuthenticator()
        {
            string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            var user = await _userManager.GetUserAsync(User);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            var token = await _userManager.GetAuthenticatorKeyAsync(user);
            string AuthenticatorUri = string.Format(AuthenticatorUriFormat, _urlEncoder.Encode("OceansApp"),
                _urlEncoder.Encode(user.Email), token);
            var model = new TwoFactorAuthenticationVM() { Token = token, QRCodeUrl = AuthenticatorUri };

            if (user.TwoFactorEnabled)
            {
                ViewData["TwoFactorNoEnabled"] = user.TwoFactorEnabled;
            }
            else
            {
                ViewData["TwoFactorNoEnabled"] = true;
            }
            ViewData["Title"] = "Enable 2-factor authentication";
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EnableAuthenticator(TwoFactorAuthenticationVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.TwoFactorEnabled)
            {
                ViewData["TwoFactorNoEnabled"] = user.TwoFactorEnabled;
            }
            else
            {
                ViewData["TwoFactorNoEnabled"] = true;
            }
            if (ModelState.IsValid)
            {
                var succeeded = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code);
                if (succeeded)
                {
                    await _userManager.SetTwoFactorEnabledAsync(user, true);
                    DeleteCookie(".AspNetCore.Identity.Application");
                    DeleteCookie("Identity.TwoFactorRememberMe");
                    DeleteCookie("ai_user");
                    DeleteCookie(".AspNetCore.Antiforgery.ZPQcRgzyRNU");
                }
                else
                {
                    ModelState.AddModelError("Code", "The code you entered is incorrect.");
                    return View(model);
                }
                return RedirectToAction(nameof(AuthenticatorConfirmation));
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult AuthenticatorConfirmation()
        {
            ViewData["TwoFactorNoEnabled"] = true;
            ViewData["Title"] = "Confirmation Authentication";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyAuthenticatorCode(bool rememberMe, string? returnUrl)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.Content("~/");
            }
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Authentication Verification";
            return View(new VerifyAuthenticatorVM { ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyAuthenticatorCode(VerifyAuthenticatorVM model)
        {
            model.ReturnUrl = model.ReturnUrl ?? Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe, rememberClient: true);

            if (result.Succeeded)
            {
                return LocalRedirect(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError("Code", "The code you entered is incorrect.");
                return View(model);
            }
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            ViewData["Title"] = "Forgot Password";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackurl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code, email = model.Email }, protocol: HttpContext.Request.Scheme);
                EmailTemplates emailTemplates = new();
                var userDetails = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == user.Id);
                if (userDetails == null)
                {
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                var forgotPassBody = emailTemplates.ForgotPasswordBody(callbackurl, userDetails.Name);
                var templateEmail = emailTemplates.EmailTemplate("RESET YOUR PASSWORD", forgotPassBody);
                SendEmailVM emailModel = new()
                {
                    Subject = "Change Password",
                    EmailTo = model.Email,
                    Body = templateEmail,
                    SharedEmailFrom = Environment.GetEnvironmentVariable(_config["sharedEmailOceansApp"])
                };
                _backgroundTaskQueue.QueueBackgroundWorkItem(async (scopeFactory, token) =>
                {
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var sendEmail = scope.ServiceProvider.GetRequiredService<ISendEmailRepository>();
                        try
                        {
                            string? result = await sendEmail.SendEmail(emailModel);
                        }
                        catch (Exception ex)
                        {
                           //Log the error
                        }
                    }
                });
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            ViewData["Title"] = "Forgot Password";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code, string email)
        {
            ViewData["Title"] = "Password Change";
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }
                AddErrors(result);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            ViewData["Title"] = "Confirmation Password Change";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreatePasswordConfirmation()
        {
            ViewData["Title"] = "Confirmation Password Creation";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreatePassword(string code, string email)
        {
            ViewData["Title"] = "Create Password";
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePassword(ResetPasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("CreatePasswordConfirmation");
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("CreatePasswordConfirmation");
                }
                AddErrors(result);
            }

            return View(model);
        }


        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public void DeleteCookie(string cookieName)
        {
            var cookie = _httpContextAccessor.HttpContext.Request.Cookies[cookieName];

            if (cookie != null)
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true,
                    Secure = true, // Use 'true' if your application is using HTTPS.
                    SameSite = SameSiteMode.Strict
                };

                _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, "", cookieOptions);
            }
        }
    }
}
