using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.ObjectModel;
using OceansApp.Models.Models;
using OceansApp.Utility;
using OceansApp.Utility.LazyLoading;
using Microsoft.AspNetCore.Cors;

namespace OceansApp.Areas.AdminCenter.Controllers
{
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    [Authorize(Policy = "AccessToUserAdministration")]
    public class ApplicationUserController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly LazyServiceProvider<ISendEmailRepository> _sendEmail;
        public ApplicationUserController(IConfiguration config, IUnitOfWork unitOrWork, UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager, LazyServiceProvider<ISendEmailRepository> emailSender)
        {
            _config = config;
            _unitOfWork = unitOrWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _sendEmail = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
        [Authorize(Roles = "Master")]
        public IActionResult Register()
        {
            List<SelectListItem> roleList = new List<SelectListItem>();
            roleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            }).ToList();

            RegisterVM registerVM = new()
            {
                RoleList = roleList
            };

            return View(registerVM);
        }

        [HttpPost]
        [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var userCategory =  await _unitOfWork.ApplicationUserCategory.GetFirstOrDefaultAsync(x => x.Name == "Administrative");
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    LastName = model.LastName,
                    Occupation = model.Occupation,
                    IsActive = true,
                    PhoneNumber = model.PhoneNumber,
                    UserCategoryId = userCategory.UserCategoryId
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (model.Role == null)
                    {
                        await _userManager.AddToRoleAsync(user, "Simple");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackurl = Url.Action("ConfirmEmail", "Account", new { area = "", userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    var emailToSend = new SendEmailVM();
                    emailToSend.Subject = "Confirma tu cuenta - Oceans App";
                    emailToSend.SharedEmailFrom = Environment.GetEnvironmentVariable(_config["sharedEmailOceansApp"]);
                    emailToSend.EmailTo = model.Email;
                    emailToSend.Body = "Confirma tu cuenta haciendo click <a href=\"" + callbackurl + "\">Aquí</a>";
                    await _sendEmail.Value.SendEmail(emailToSend);
                    TempData["success"] = "El usuario para " + model.Name + " fue creado con exito. Se le envió una confirmación a su correo.";
                    return RedirectToAction("Index");
                }
                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                    {
                        ModelState.AddModelError("", $"El usuario '{model.Email}' ya existe. Por favor, intente con otro usuario.");
                    }
                    else
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }
        public async Task<IActionResult> Edit(string userId)
        {
            var userFromDb = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == userId);
            var role = _userManager.GetRolesAsync(userFromDb).Result;

            var roles = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            });

            //ADD ROLE var result = _userManager.AddToRoleAsync(userFromDb, "Master").GetAwaiter().GetResult();
            //REMOVE ROLE var result = _userManager.RemoveFromRoleAsync(userFromDb, "Master").GetAwaiter().GetResult();

            ApplicationUserVM userToUpdate = new() {
            Id = userId,
            Email = userFromDb.Email,
            Name = userFromDb.Name,
            LastName = userFromDb.LastName,
            Ocupation = userFromDb.Occupation,
            PhoneNumber = userFromDb.PhoneNumber,
            Role = role[0],
            Roles = roles.ToList()
            };
            return View(userToUpdate);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(ApplicationUserVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userToUpdate = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == model.Id);
                    var actualUserRole = _userManager.GetRolesAsync(userToUpdate).Result;
                    if (actualUserRole[0] != model.Role)
                    {
                        _userManager.RemoveFromRoleAsync(userToUpdate, actualUserRole[0]).GetAwaiter().GetResult();
                        _userManager.AddToRoleAsync(userToUpdate, model.Role).GetAwaiter().GetResult();
                    }
                    userToUpdate.Name = model.Name;
                    userToUpdate.LastName = model.LastName;
                    userToUpdate.Occupation = model.Ocupation;
                    userToUpdate.PhoneNumber = model.PhoneNumber;

                   await _unitOfWork.SaveAsync();

                    TempData["success"] = "¡Los datos fueron guardados con éxito!";
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            return View("Edit", model);
        }

        //API CALLS REGION
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {  
                Collection<ApplicationUserVM> usersList = new Collection<ApplicationUserVM>();

                var users = await _unitOfWork.ApplicationUser.GetAllAsync();
                foreach (var user in users)
                { 
                    ApplicationUserVM customUser = new()
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Name = user.Name,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber,
                        Ocupation = user.Occupation,
                        IsActive = user.IsActive,
                        TwoFactorEnabled = user.TwoFactorEnabled
                    };
                    usersList.Add(customUser);
                }
                return Json(new { data = usersList });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_User_Master)]
        [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
        public async Task<IActionResult> RemoveAuthenticator(string userId)
        {
            try {
                var user = await _userManager.FindByIdAsync(userId);
                await _userManager.ResetAuthenticatorKeyAsync(user);
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                return Json(new { success = true, message = "¡La autenticación de dos factores fue desactivada con éxito!" });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //POST
        [HttpPost]
        public async Task<IActionResult> ActivateDeactivate(String userId)
        {
            try
            {
                var message = "";
                var userToUpdate = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == userId);
                if (userToUpdate.IsActive == true)
                {
                    userToUpdate.IsActive = false;
                    userToUpdate.LockoutEnd = DateTime.Now.AddYears(1000);
                    message = "¡El usuario fue desactivado con éxito!";
                }
                else
                {
                    userToUpdate.IsActive = true;
                    userToUpdate.LockoutEnd = DateTime.Now;
                    message = "¡El usuario fue activado con éxito!";
                }
                await _unitOfWork.SaveAsync();
                return Json(new { success = true, message = message });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

    }
}
