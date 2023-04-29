using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.Models.Models;

namespace FinancialCalculatorWeb.Areas.Finances.Controllers
{
    [Authorize(Roles = SD.Role_User_Master + "," + SD.Role_User_Admin)]
    [RequireTwoFactorEnabled]
    public class AccountingAccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountingAccountController(IUnitOfWork _unitOfWork)
        {
            _unitOfWork = _unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<AccountingAccount> objCuentaContableList = _unitOfWork.AccountingAccounts.GetAll();
            return View(objCuentaContableList);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AccountingAccount obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.AccountingAccounts.Add(obj);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [ValidateAntiForgeryToken]
        public IActionResult Edit(string? idCuentaContable)
        {
            if(idCuentaContable==null || idCuentaContable == "")
            {
                return NotFound();
            }
            var cuentaContableFromDbFirst = _unitOfWork.AccountingAccounts.GetFirstOrDefault(u => u.IdAccountingAccount == idCuentaContable);
            if(cuentaContableFromDbFirst == null)
            {
                return NotFound();
            }
            return View(cuentaContableFromDbFirst);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AccountingAccount obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.AccountingAccounts.Update(obj);
                _unitOfWork.Save();
            }
            return View(obj);
        }
    }
}
