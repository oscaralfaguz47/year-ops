using OceansApp.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialCalculatorWeb.Areas.Finances.Controllers
{
    [ApiController]
    [Route("Finances/[controller]")]
    [Area("Finances")]
    [Authorize]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class AccountingAccountsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountingAccountsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Policy = "AccessToAccountingAccountsList")]
        [HttpGet("GetAccountingAccountsListWhereCostCenterId")]
        public async Task<IActionResult> GetAccountingAccountsListWhereCostCenterId(int costCenterId)
        {
            try
            {
                var accountingAccounts = await _unitOfWork.AccountingAccounts.GetAccountingAccountsWhereCostCenterIdAsync(costCenterId);
                return Ok(new
                {
                    AccountingAccounts = accountingAccounts
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

    }
}
