using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels;
using OceansAppWeb.Controllers;
using System.Text.Encodings.Web;

namespace OceansAppWeb.Account.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly UrlEncoder _urlEncoder;
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender
            , UrlEncoder urlEncoder, ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOrWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _urlEncoder = urlEncoder;
            _dbContext = dbContext;
            _roleManager = roleManager;
            _unitOfWork = unitOrWork;
            _httpContextAccessor = httpContextAccessor;
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
            if (userFromDb.TwoFactorEnabled)
            {
                ViewData["TwoFactorEnabled"] = true;
            }
            else
            {
                ViewData["TwoFactorEnabled"] = userFromDb.TwoFactorEnabled;
            }
            ProfileVM myInfo = new()
            {
                Id = userFromDb.Id,
                Email = userFromDb.Email,
                Name = userFromDb.Name,
                LastName = userFromDb.LastName,
                Ocupation = userFromDb.Occupation,
                PhoneNumber = userFromDb.PhoneNumber
            };
            ViewData["Title"] = "Mi Perfil";
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

                    TempData["success"] = "¡Los datos fueron guardados con éxito!";
                    if (userToUpdate.TwoFactorEnabled)
                    {
                        ViewData["TwoFactorEnabled"] = true;
                    }
                    else
                    {
                        ViewData["TwoFactorEnabled"] = userToUpdate.TwoFactorEnabled;
                    }
                    return View("Profile", model);
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            var userFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == model.Id);
            if (userFromDb.TwoFactorEnabled)
            {
                ViewData["TwoFactorNoEnabled"] = true;
            }
            else
            {
                ViewData["TwoFactorNoEnabled"] = userFromDb.TwoFactorEnabled;
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
            ViewData["Title"] = "Confirmación de Correo";
            return View(result.Succeeded ? "ConfirmEmail" : "Error");

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
            ViewData["ReturnUrl"] = returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    if (user != null)
                    {
                        if (await _userManager.CheckPasswordAsync(user, model.Password))
                        {
                            if (user.EmailConfirmed == false)
                            {
                                ModelState.AddModelError(string.Empty, "Aún no haz confirmado tu correo, por favor ingresa a tu correo y confirmalo con el email que te hemos enviado.");
                                return View(model);
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
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Tu usuario o contraseña son incorrectos.");
                                return View(model);
                            }
                        }
                        else
                        {
                            user.AccessFailedCount++;
                            await _userManager.UpdateAsync(user);
                            ModelState.AddModelError(string.Empty, "Tu usuario o contraseña son incorrectos.");
                            return View(model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Tu usuario no se encuentra registrado, ponte en contacto con el Administrador.");
                        return View(model);
                    }
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            return View(model);
        }

        [HttpGet]
        [Authorize]
        [RequireTwoFactorEnabled]
        public async Task<IActionResult> RemoveAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            return RedirectToAction(nameof(HomeController.Dashboard), "Home");
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
            ViewData["Title"] = "Habilitar Autenticación de 2 factores";
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
                    ModelState.AddModelError("Code", "El código que ingresaste es incorrecto.");
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
            ViewData["Title"] = "Confirmacion Autenticación";
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
            ViewData["Title"] = "Verificación de Autenticación";
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
                ModelState.AddModelError("Code", "El código que ingresaste es incorrecto.");
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
            ViewData["Title"] = "Olvidé Contraseña";
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
                var callbackurl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                await _emailSender.SendEmailAsync(model.Email, "Cambiar Contraseña - Oceans App",
                    "Cambia tu contraseña haciendo click: <a href=\"" + callbackurl + "\">Aquí</a>");

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            ViewData["Title"] = "Olvidé Contraseña";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code)
        {
            ViewData["Title"] = "Cambio de Contraseña";
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
            ViewData["Title"] = "Confirmación Cambio de Contraseña";
            return View();
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
                    Secure = true, // Utiliza 'true' si tu aplicación está utilizando HTTPS.
                    SameSite = SameSiteMode.Strict
                };

                _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, "", cookieOptions);
            }
        }
    }
}
