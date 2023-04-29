using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels;
using OceansApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.ObjectModel;

namespace OceansApp.Areas.AdminCenter.Controllers
{
    [Area("AdminCenter")]
    [Authorize(Roles = SD.Role_User_Master)]
    [RequireTwoFactorEnabled]
    public class ApplicationUserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ApplicationUserController(IUnitOfWork unitOrWork, UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOrWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Edit(string userId)
        {
            var userFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
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
        public IActionResult Update(ApplicationUserVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userToUpdate = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == model.Id);
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

                    _unitOfWork.Save();

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
        public IActionResult GetAll()
        {
            try
            {  
                Collection<ApplicationUserVM> usersList = new Collection<ApplicationUserVM>();

                var users = _unitOfWork.ApplicationUser.GetAll();
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
                        IsActive = user.IsActive 
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

        //POST
        [HttpPost]
        public IActionResult ActivateDeactivate(String userId)
        {
            try
            {
                var message = "";
                var userToUpdate = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
                if (userToUpdate.IsActive == true)
                {
                    userToUpdate.IsActive = false;
                    message = "¡El usuario fue desactivado con éxito!";
                }
                else
                {
                    userToUpdate.IsActive = true;
                    message = "¡El usuario fue activado con éxito!";
                }
                _unitOfWork.Save();
                return Json(new { success = true, message = message });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}
