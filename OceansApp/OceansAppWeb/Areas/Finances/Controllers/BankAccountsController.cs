using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [ApiController]
    [Route("Finances/[controller]")]
    [Area("Finances")]
    [Authorize]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class BankAccountsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BankAccountsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToBankAccountsList")]
        [HttpGet("GetBankAccountsByPaymentMethodList")]
        public async Task<IActionResult> GetBankAccountsByPaymentMethodList(int paymentMethodId)
        {
            try
            {
                List<GetDataForSelectVM> bankAccountsList = await _unitOfWork.BankAccount.GetBankAccountsWherePaymentMethod(paymentMethodId);
                return Ok(new
                {
                    BankAccounts = bankAccountsList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

    }
}
