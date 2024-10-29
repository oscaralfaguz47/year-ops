using Azure.Storage.Queues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Models.ViewModels.Account;
using OceansApp.Models.ViewModels.Blobs;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Utility;
using OceansApp.Utility.NotificationTemplates;
using OceansApp.Utility.SharedMethods;
using OceansApp.Utility.SharedMethods.InputValidations;
using OceansAppWeb.Controllers;
using System.Security.Claims;
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
        private readonly Lazy<QueueClient> _queueClient;
        private readonly IMemoryCache _cache;
        private readonly Lazy<IAzureBlobRepository> _azureBlobRepository;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager
            , UrlEncoder urlEncoder, ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOrWork,
            IHttpContextAccessor httpContextAccessor, IConfiguration config,
            IMemoryCache cache, Lazy<QueueClient> queueClient, Lazy<IAzureBlobRepository> azureBlobRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
            _dbContext = dbContext;
            _roleManager = roleManager;
            _unitOfWork = unitOrWork;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _queueClient = queueClient;
            _cache = cache;
            _azureBlobRepository = azureBlobRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
        public IActionResult ProfileAsync()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
        public async Task<IActionResult> GetProfileInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userFromDb = await _unitOfWork.ApplicationUser.GetUserProfileDataAsync(userId);

            return Ok(new
            {
                profileInfo = userFromDb
            });
        }

        [HttpPost]
        [Authorize]
        [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileVM model)
        {
            if (model == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
            }
            ValidateInputs validateInputs = new();

            validateInputs.ValidateRequiredFieldAnyValue("Id", "UserId", model.Id, ModelState);
            validateInputs.ValidateRequiredAndStringLength("Name", "Name", model.Name, 100, ModelState);
            validateInputs.ValidateRequiredAndStringLength("LastName", "Last Name", model.LastName, 150, ModelState);
            validateInputs.ValidateNotRequiredAndStringLength("PhoneNumber", "Phone Number", model.PhoneNumber, 100, ModelState);
            validateInputs.ValidateNotRequiredAndStringLength("Occupation", "Occupation", model.Occupation, 100, ModelState);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                 .Select(e => e.ErrorMessage)
                                                 .ToList();
                return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", errors = errors });
            }
            try
            {
                var userToUpdate = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == model.Id);

                userToUpdate.Name = model.Name;
                userToUpdate.LastName = model.LastName;
                userToUpdate.Occupation = model.Occupation;
                userToUpdate.PhoneNumber = model.PhoneNumber;

                await _unitOfWork.SaveAsync();

                return Ok(new
                {
                    success = true,
                    message = "Changes saved!"
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { error = e.Message, messageType = "Exception Error" });
            }

        }

        [Authorize]
        [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeProfilePhoto([FromForm] IFormFile file)
        {
            try
            {
                // Validate file input
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredFile("Photo", "Photo", file, ModelState);
                validateInputs.ValidateValidFile("Photo", file, ModelState);

                // Check if the ModelState has any errors after validations
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return BadRequest(new { errors = errors, messageType = "Validation Error" });
                }

                // Get the current user's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                string containerId = "user-profile-photos";
                string entityType = "UserProfile";

                // Check if the file already exists in ImageBlob
                ImageBlob fileAlreadyExists = await _unitOfWork.ApplicationUser.VerifyIfUploadedFileAsync(file, userId, containerId, entityType);

                if (fileAlreadyExists != null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "You changed your profile photo!"
                    });
                }

                // Prepare the file list for upload
                List<IFormFile> fileList = new();
                fileList.Add(file);

                // Try uploading the file to Azure Blob Storage
                List<BlobUploadResult> uploadedBlob;
                try
                {
                    uploadedBlob = await _azureBlobRepository.Value.UploadFilesAsync(containerId, fileList, userId.Substring(0, 8), 10950);

                    // If the upload was successful, proceed to save the blob details in the database
                    if (uploadedBlob[0].Success)
                    {
                        var oldImage = await _unitOfWork.ImageBlob
    .GetFirstOrDefaultAsync(x => x.ContainerName == containerId && x.EntityId == userId && x.EntityType == entityType);

                        ImageBlob imageToSave = new()
                        {
                            BlobName = uploadedBlob[0].FileName,
                            ContainerName = uploadedBlob[0].ContainerId,
                            BlobUrl = uploadedBlob[0].BlobUrl,
                            CreationDate = DateTime.UtcNow,
                            EntityId = userId,
                            EntityType = "UserProfile"
                        };
                        var transaction = await _unitOfWork.BeginTranAsync();
                        await _unitOfWork.ImageBlob.AddAsync(imageToSave);
                        await _unitOfWork.SaveAsync();

                        //Delete image from Azure
                        if (oldImage != null)
                        {
                            MethodResponse deleteResponse = await _azureBlobRepository.Value.DeleteBlobAsync(containerId, oldImage.BlobName);

                            if (!deleteResponse.Success)
                            {
                                await transaction.RollbackAsync();
                            }
                            else
                            {
                                _unitOfWork.ImageBlob.Remove(oldImage);
                                await _unitOfWork.SaveAsync();
                                await transaction.CommitAsync();
                            }
                        }
                        else
                        {
                            await transaction.CommitAsync();
                        }

                        return Ok(new
                        {
                            success = true,
                            message = "You changed your profile photo!"
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Handle any errors during the upload or database save process
                    return BadRequest(new { error = $"The image couldn't be uploaded or saved: {ex.Message}", messageType = "Exception Error" });
                }
            }
            catch (Exception ex)
            {
                // Catch any unexpected errors during the process
                return BadRequest(new { error = $"An error occurred: {ex.Message}", messageType = "Exception Error" });
            }

            // If we reach here, something went wrong
            return BadRequest(new { error = "Something went wrong", messageType = "Unknown Error" });
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return View("Error");
            }
            var parts = code.Split(':');
            if (parts.Length != 2)
            {
                return View("Error");
            }

            var userId = parts[0];
            var confirmationCode = parts[1];
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, confirmationCode);
            if (result.Succeeded)
            {
                var resetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
                return RedirectToAction("CreatePassword", "Account", new { code = resetCode });
            }
            else
            {
                var errors = result.Errors.ToList();
                if (errors.Any(e => e.Code.Contains("InvalidToken")))
                {
                    if (!user.EmailConfirmed)
                    {
                        var newCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var resultNewCode = await _userManager.ConfirmEmailAsync(user, newCode);
                        var callbackurl = Url.Action("ConfirmEmail", "Account", new { area = "", code = user.Id + ":" + newCode }, protocol: HttpContext.Request.Scheme);
                        EmailTemplates emailTemplates = new();
                        var createPassBody = emailTemplates.CreatePasswordBody(callbackurl, user.UserName.Trim());
                        var templateEmail = emailTemplates.EmailTemplate("CREATE YOUR PASSWORD", createPassBody);
                        SendEmailVM emailModel = new()
                        {
                            Subject = "Create your account - Ripple by Oceans",
                            EmailTo = user.Email.Trim(),
                            Body = templateEmail,
                            SharedEmailFrom = _config["SharedMailboxEmailRippleApp"]
                        };

                        try
                        {
                            string message = JsonConvert.SerializeObject(emailModel);
                            await _queueClient.Value.SendMessageAsync(StringsMethods.Base64Encode(message));
                        }
                        catch (Exception ex)
                        {
                            //Log the error
                        }

                        InvalidToken invalidTokenModelCreatePassword = new InvalidToken();
                        invalidTokenModelCreatePassword.Title = "Your invite has been expired!";
                        invalidTokenModelCreatePassword.Message = "We just sent you another invite. Please check your email";
                        return View("InvalidToken", invalidTokenModelCreatePassword);
                    }
                    else
                    {
                        InvalidToken invalidTokenExpiredModel = new InvalidToken();
                        invalidTokenExpiredModel.Title = "You have already corfirmed your email!";
                        invalidTokenExpiredModel.Message = "Reset your password if you need it.";
                        invalidTokenExpiredModel.ButtonText = "Ok, Reset my password";
                        return View("InvalidToken", invalidTokenExpiredModel);
                    }
                }
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
                        var applicationUser = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == user.Id);
                        if (applicationUser.IsActive)
                        {
                            var currentUserClaims = await _userManager.GetClaimsAsync(user);
                            var existingClaimTwoFactorRequired = currentUserClaims.FirstOrDefault(c => c.Type == "TwoFactorRequired");
                            var existingClaimTwoFactorEnabled = currentUserClaims.FirstOrDefault(c => c.Type == "amr");

                            var claimTwoFactorRequired = new Claim("TwoFactorRequired", applicationUser.TwoFactorRequired.ToString());
                            var claimTwoFactorEnabled = new Claim("amr", user.TwoFactorEnabled ? "mfa" : "");

                            if (existingClaimTwoFactorRequired == null)
                            {
                                await _userManager.AddClaimAsync(user, claimTwoFactorRequired);
                            }
                            else if (existingClaimTwoFactorRequired.Value != applicationUser.TwoFactorRequired.ToString())
                            {
                                await _userManager.RemoveClaimAsync(user, existingClaimTwoFactorRequired);
                                await _userManager.AddClaimAsync(user, claimTwoFactorRequired);
                            }

                            if (existingClaimTwoFactorEnabled == null)
                            {
                                await _userManager.AddClaimAsync(user, claimTwoFactorEnabled);
                            }
                            else if (existingClaimTwoFactorEnabled.Value != (user.TwoFactorEnabled ? "mfa" : ""))
                            {
                                await _userManager.RemoveClaimAsync(user, existingClaimTwoFactorEnabled);
                                await _userManager.AddClaimAsync(user, claimTwoFactorEnabled);
                            }

                            // Regenerate the authentication ticket
                            //await _signInManager.RefreshSignInAsync(user);

                            GenerateUserSessionChangesExpirationCache(user.Id);

                            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                            if (result.Succeeded)
                            {
                                if (!user.TwoFactorEnabled && applicationUser.TwoFactorRequired)
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

        private void GenerateUserSessionChangesExpirationCache(string userId)
        {

            var cacheKey = $"UserSessionChangesExpiration_{userId}";
            _cache.Set(cacheKey, DateTimeOffset.Now.AddMinutes(SD.cacheExpirationTimeInSeconds), new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(SD.cacheExpirationTimeInSeconds)
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EnableAuthenticator()
        {
            string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            var user = await _userManager.GetUserAsync(User);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            var token = await _userManager.GetAuthenticatorKeyAsync(user);
            string appName = _config["TwoFactorAppNameENV"];
            string AuthenticatorUri = string.Format(AuthenticatorUriFormat, _urlEncoder.Encode(appName),
                _urlEncoder.Encode(user.Email), token);
            var model = new TwoFactorAuthenticationVM() { Token = token, QRCodeUrl = AuthenticatorUri };

            if (user.TwoFactorEnabled)
            {
                ViewData["TwoFactorNoEnabled"] = user.TwoFactorEnabled;
                return LocalRedirect("/Home/Dashboard");
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var cacheKey = $"UserSessionChangesExpiration_{user.Id}";
                _cache.Remove(cacheKey);
            }
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
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackurl = Url.Action("ResetPassword", "Account",
                    new { code = code }, protocol: HttpContext.Request.Scheme);
                EmailTemplates emailTemplates = new();
                var userDetails = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == user.Id);
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
                    SharedEmailFrom = _config["SharedMailboxEmailRippleApp"],
                };

                try
                {
                    string message = JsonConvert.SerializeObject(emailModel);
                    await _queueClient.Value.SendMessageAsync(StringsMethods.Base64Encode(message));
                }
                catch (Exception ex)
                {
                    //Log the error
                }

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
        public IActionResult ResetPassword(string code)
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
                    ModelState.AddModelError("Email", "Your Email is incorrect");
                    return View(model);
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }
                else
                {
                    var errors = result.Errors.ToList();
                    if (errors.Any(e => e.Code.Contains("InvalidToken")))
                    {
                        ModelState.AddModelError("Email", "Your Email is incorrect or your request has been expired");
                    }
                    else
                    {
                        return View("Error");
                    }
                }
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
        public IActionResult CreatePassword(string code)
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
                    ModelState.AddModelError("Email", "Your Email is incorrect");
                    return View(model);
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("CreatePasswordConfirmation");
                }
                ModelState.AddModelError("Email", "Your Email is incorrect");
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
