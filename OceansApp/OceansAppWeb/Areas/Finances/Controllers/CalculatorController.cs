using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.ObjectModel;
using System.Security.Claims;

namespace FinancialCalculatorWeb.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize(Roles = SD.Role_User_Master + "," + SD.Role_User_Admin + "," + SD.Role_User_Simple)]
    public class CalculatorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        public CalculatorController(IUnitOfWork unitOrWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOrWork;
            _emailSender = emailSender;
        }

        [RequireTwoFactorEnabled]
        public IActionResult Index()
        {
            Collection<CalculatorCostCenterUserConfigurationVM> costCenterUserList = new Collection<CalculatorCostCenterUserConfigurationVM>();
            IEnumerable<CostCenter> costCenterList = _unitOfWork.CenterOfCosts.GetCostCenterOfExpenses().OrderBy(x => x.Description);
            foreach (var costCenter in costCenterList)
            {
                CalculatorCostCenterUserConfigurationVM costCenterUserObj = new()
                {
                    CostCenterId = costCenter.CostCenterId,
                    Description = costCenter.Description,
                    Detail = costCenter.Detail,
                    Active = true,
                    CompanyId = costCenter.CompanyId
                };
                costCenterUserList.Add(costCenterUserObj);
            }

            var clients = _unitOfWork.Client.GetAll(x => x.ClientCategory == "EXT" && x.IsActive == "S" && x.ClientCode != "OCELL_C0001").OrderBy(x => x.Name).Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.ClientId.ToString()
            });

            var roles = _unitOfWork.ConsultantRole.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.ConsultantRoleId.ToString()
            });
            var qualityLevels = _unitOfWork.ConsultantQualityLevel.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.ConsultantQualityLevelId.ToString()
            });

            CalculatorVM cvm = new()
            {
                ClientList = clients.ToList(),
                ConsultantRoleList = roles.ToList(),
                ConsultantQualityLevelList = qualityLevels.ToList(),
                CalculatorCostCenterUserConfigurationVM = costCenterUserList
            };
            return View("Index", cvm);
        }



        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(CalculatorVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var globalConfiguration = _unitOfWork.CalculatorGlobalConfiguration.GetGlobalConfiguration();
                    DateTime finalDate = Convert.ToDateTime("" + globalConfiguration.EndDate.Month + "/" + globalConfiguration.EndDate.Day + "/" + globalConfiguration.EndDate.Year + " 11:59:59 pm");
                    var costOfSalesAccountingAccounts = await _unitOfWork.LedgerMovements
                                   .GetAccountingAccountsWithBalance("5", globalConfiguration.StartDate, finalDate, 1, "D");
                    var expensesAccountingAccounts = await _unitOfWork.LedgerMovements
                                   .GetAccountingAccountsWithBalance("6", globalConfiguration.StartDate, finalDate, 1, "D");
                    var returnsAndDiscountsAccountingAccounts = await _unitOfWork.LedgerMovements.GetAccountingAccountsReturnsAndDiscountsWithBalance(
                        globalConfiguration.StartDate, finalDate, 1);
                    Decimal totalCostOfSales = 0;
                    Decimal totalExpenses = 0;
                    Decimal totalReturnsAndDiscounts = 0;
                    int userIsMasterOrAdmin = 0;
                    var userRoles = HttpContext.User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();

                    Double numMonths = ((finalDate - globalConfiguration.StartDate).TotalDays) / 30;

                    List<CalculatorExpensesCostsDistribution> expensesCostsDistributionList = new List<CalculatorExpensesCostsDistribution>();

                    foreach (var role in userRoles)
                    {
                        if (role == SD.Role_User_Master || role == SD.Role_User_Admin)
                        {
                            userIsMasterOrAdmin++;
                        }
                    }
                    Collection<CalculatorCostCenterUserConfigurationVM> costCenterList = new Collection<CalculatorCostCenterUserConfigurationVM>();

                    if (userIsMasterOrAdmin > 0)
                    {
                        costCenterList = model.CalculatorCostCenterUserConfigurationVM;
                    }
                    else
                    {
                        var costsCenters = _unitOfWork.CenterOfCosts.GetCostCenterOfExpenses();
                        foreach (var costCenter in costsCenters)
                        {
                            CalculatorCostCenterUserConfigurationVM costC = new()
                            {
                                CostCenterId = costCenter.CostCenterId,
                                Description = costCenter.Description,
                                Active = true
                            };
                            costCenterList.Add(costC);
                        }
                    }

                    foreach (var costCenter in costCenterList)
                    {
                        bool validateCostCenter = false;
                        int? idCostCenter = null;
                        if (userIsMasterOrAdmin > 0)
                        {
                            validateCostCenter = true;
                        }
                        if (validateCostCenter)
                        {
                            if (costCenter.Active)
                            {
                                idCostCenter = costCenter.CostCenterId;
                            }
                            else
                            {
                                idCostCenter = null;
                            }
                        }
                        else
                        {
                            idCostCenter = costCenter.CostCenterId;
                        }
                        if (idCostCenter != null)
                        {
                            //CALCULATE TOTAL COST OF SALES
                            foreach (var accountingAccount in costOfSalesAccountingAccounts)
                            {
                                if (accountingAccount.CostCenterId == idCostCenter)
                                {
                                    Decimal amountByCostCenter = 0;
                                    Decimal totalAmountByCostCenterAfterPercentage = 0;

                                    amountByCostCenter += accountingAccount.TotalAmount;
                                    Decimal percentageIncrease = (decimal)_unitOfWork.CalculatorCostCenterIncreaseConfiguration.GetFirstOrDefault(x => x.CostCenterId == costCenter.CostCenterId).Increase;
                                    totalAmountByCostCenterAfterPercentage = totalAmountByCostCenterAfterPercentage
                                        + (amountByCostCenter
                                        * ((decimal)percentageIncrease / 100));
                                    if (amountByCostCenter > 0)
                                    {
                                        expensesCostsDistributionList.Add(new CalculatorExpensesCostsDistribution
                                        {
                                            AccountingAccountId = accountingAccount.AccountingAccountId,
                                            AccountingAccountCode = accountingAccount.AccountingAccountCode,
                                            AccountingAccountName = accountingAccount.AccountingAccountName,
                                            Amount = (((decimal)amountByCostCenter + (decimal)totalAmountByCostCenterAfterPercentage) / (decimal)numMonths) / (decimal)globalConfiguration.PeopleNumber,
                                            CostCenterName = costCenter.Description,
                                            increasePercentage = percentageIncrease,
                                            increaseAmount = ((decimal)totalAmountByCostCenterAfterPercentage / (decimal)numMonths) / (decimal)globalConfiguration.PeopleNumber,
                                            CompanyId = costCenter.CompanyId
                                        });
                                    }
                                    totalCostOfSales = totalCostOfSales + amountByCostCenter + totalAmountByCostCenterAfterPercentage;
                                }
                            }
                            //CALCULATE TOTAL EXPENSES
                            foreach (var accountingAccount in expensesAccountingAccounts)
                            {
                                if (accountingAccount.CostCenterId == idCostCenter)
                                {
                                    Decimal amountByCostCenter = 0;
                                    Decimal totalAmountByCostCenterAfterPercentage = 0;

                                    amountByCostCenter += accountingAccount.TotalAmount;

                                    Decimal percentageIncrease = (decimal)_unitOfWork.CalculatorCostCenterIncreaseConfiguration.GetFirstOrDefault(x => x.CostCenterId == costCenter.CostCenterId).Increase;
                                    totalAmountByCostCenterAfterPercentage = totalAmountByCostCenterAfterPercentage
                                        + (amountByCostCenter * ((decimal)percentageIncrease / 100));
                                    if (amountByCostCenter > 0)
                                    {
                                        if (userIsMasterOrAdmin > 0)
                                        {
                                            expensesCostsDistributionList.Add(new CalculatorExpensesCostsDistribution
                                            {
                                                AccountingAccountId = accountingAccount.AccountingAccountId,
                                                AccountingAccountCode = accountingAccount.AccountingAccountCode,
                                                AccountingAccountName = accountingAccount.AccountingAccountName,
                                                Amount = (((decimal)amountByCostCenter + (decimal)totalAmountByCostCenterAfterPercentage) / (decimal)numMonths) / (decimal)globalConfiguration.PeopleNumber,
                                                CostCenterName = costCenter.Description,
                                                increasePercentage = percentageIncrease,
                                                increaseAmount = ((decimal)totalAmountByCostCenterAfterPercentage / (decimal)numMonths) / (decimal)globalConfiguration.PeopleNumber,
                                                CompanyId = costCenter.CompanyId
                                            });
                                        }
                                    }
                                    totalExpenses = totalExpenses + amountByCostCenter + totalAmountByCostCenterAfterPercentage;
                                }
                            }
                        }
                    }
                    //CALCULATE TOTAL RETURNS AND DISCOUNTS
                    foreach (var accountingAccount in returnsAndDiscountsAccountingAccounts)
                    {
                        Decimal amount = 0;

                        amount += accountingAccount.TotalAmount;
                        if (amount > 0)
                        {
                            if (userIsMasterOrAdmin > 0)
                            {
                                expensesCostsDistributionList.Add(new CalculatorExpensesCostsDistribution
                                {
                                    AccountingAccountId = accountingAccount.AccountingAccountId,
                                    AccountingAccountCode = accountingAccount.AccountingAccountCode,
                                    AccountingAccountName = accountingAccount.AccountingAccountName,
                                    Amount = ((decimal)amount / (decimal)numMonths) / (decimal)globalConfiguration.PeopleNumber,
                                    CostCenterName = "NO ASIGNADO A CENTRO DE COSTO",
                                    increasePercentage = 0,
                                    increaseAmount = 0,
                                    CompanyId = "OCE"
                                });
                            }
                        }
                        totalReturnsAndDiscounts = totalReturnsAndDiscounts + amount;
                    }
                    //PRINCIPAL COST OF SALES
                    Decimal consultantMonthlyPayment = (decimal)model.PaymentAmount;
                    double daysYear = 0;
                    double vacationDays = 0;
                    if (model.DaysYear != null)
                    {
                        daysYear = (double)model.DaysYear;
                    }
                    if (model.VacationDays != null)
                    {
                        vacationDays = (double)model.VacationDays;
                    }
                    Decimal consultantHolidaysAmount = ((decimal)consultantMonthlyPayment / (decimal)globalConfiguration.NumLaborDaysInMonth) * (decimal)daysYear;
                    Decimal consultantVacationsAmount = ((decimal)consultantMonthlyPayment / (decimal)globalConfiguration.NumLaborDaysInMonth) * (decimal)vacationDays;

                    Decimal subTotalMonthlyAmountPayToConsultant = (consultantMonthlyPayment + (consultantHolidaysAmount / 12) + (consultantVacationsAmount / 12));

                    Decimal subtotalExpenses = ((totalCostOfSales + totalExpenses + totalReturnsAndDiscounts) / (decimal)numMonths) / globalConfiguration.PeopleNumber;
                    Decimal appliedAmountGlobalIncrease = ((subtotalExpenses) * ((decimal)globalConfiguration.AdditionalGlobalIncrease / 100));
                    Decimal totalAmountOfExpensesAndCosts = subTotalMonthlyAmountPayToConsultant + subtotalExpenses + (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease) / 100);

                    var clientRateAndConsultantAmount = _unitOfWork.ConsultantRoleQualityLevel.GetFirstOrDefault(x =>
                    x.ConsultantRoleId == int.Parse(model.ConsultantRoleId) && x.ConsultantQualityLevelId == int.Parse(model.ConsultantQualityLevelId));
                    bool isProfitLessThanConfig = false;
                    Decimal recommendedAmountToPayToConsultant = 0;
                    Decimal recommendedAmountHolidaysToConsultant = 0;
                    Decimal recommendedAmountVacationsToConsultant = 0;

                    Decimal greenProfitAmount = 0;
                    Decimal greenProfitPercentage = 0;
                    Decimal maxProfitSetPercentage = 0;
                    Decimal yellowProfitAmount = 0;
                    Decimal yellowProfitPercentage = 0;

                    var client = _unitOfWork.Client.GetFirstOrDefault(x => x.ClientId == int.Parse(model.Client));
                    if (client != null)
                    {
                        if (client.ClientClass == "A")
                        {
                            Decimal monthlyRateGreenAAA = (totalAmountOfExpensesAndCosts / (100 - (decimal)globalConfiguration.ProfitGreenClientAAA)) * 100;
                            Decimal monthlyRateYellowAAA = (totalAmountOfExpensesAndCosts / (100 - (decimal)globalConfiguration.ProfitYellowClientAAA)) * 100;
                            Decimal hourRateGreenAAA = monthlyRateGreenAAA / ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                            Decimal hourRateYellowAAA = monthlyRateYellowAAA / ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                            maxProfitSetPercentage = (decimal)globalConfiguration.ProfitGreenClientAAA;
                            if (hourRateGreenAAA > clientRateAndConsultantAmount.ClientRateMaximumAmount)
                            {
                                hourRateGreenAAA = clientRateAndConsultantAmount.ClientRateMaximumAmount;
                                monthlyRateGreenAAA = (hourRateGreenAAA * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8));
                                Decimal totalAmountPayToConsultant = 0;
                                if (((monthlyRateGreenAAA - totalAmountOfExpensesAndCosts) / monthlyRateGreenAAA) < ((decimal)globalConfiguration.MinimumGlobalProfit) / 100)
                                {
                                    totalAmountPayToConsultant = (decimal)monthlyRateGreenAAA - (((decimal)monthlyRateGreenAAA * ((decimal)globalConfiguration.MinimumGlobalProfit) / 100) + (subtotalExpenses + (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease) / 100)));
                                    Decimal numHoursInMonth = (decimal)globalConfiguration.NumLaborDaysInMonth * 8;
                                    Decimal numHolidaysHours = (decimal)daysYear * 8;
                                    Decimal numVacationHours = (decimal)vacationDays * 8;
                                    Decimal hourPriceToConsultant = (totalAmountPayToConsultant / (numHoursInMonth + (numHolidaysHours / 12) + (numVacationHours / 12)));

                                    recommendedAmountToPayToConsultant = hourPriceToConsultant * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                                    recommendedAmountHolidaysToConsultant = hourPriceToConsultant * ((decimal)daysYear * 8);
                                    recommendedAmountVacationsToConsultant = hourPriceToConsultant * ((decimal)vacationDays * 8);
                                    subTotalMonthlyAmountPayToConsultant = recommendedAmountToPayToConsultant + (recommendedAmountHolidaysToConsultant / 12) + (recommendedAmountVacationsToConsultant / 12);
                                    totalAmountOfExpensesAndCosts = subTotalMonthlyAmountPayToConsultant + subtotalExpenses + (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease / 100));
                                    isProfitLessThanConfig = true;

                                    greenProfitAmount = (monthlyRateGreenAAA - totalAmountPayToConsultant - subtotalExpenses - (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease / 100)));
                                    greenProfitPercentage = ((greenProfitAmount / monthlyRateGreenAAA) * 100);
                                    yellowProfitAmount = (((monthlyRateGreenAAA + (((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)))) / 2) - totalAmountPayToConsultant - subtotalExpenses - (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease / 100)));
                                    yellowProfitPercentage = (yellowProfitAmount / ((monthlyRateGreenAAA + (((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)))) / 2)) * 100;
                                }
                                else
                                {
                                    greenProfitAmount = (monthlyRateGreenAAA - totalAmountOfExpensesAndCosts);
                                    greenProfitPercentage = (greenProfitAmount / monthlyRateGreenAAA) * 100;
                                    yellowProfitAmount = (((monthlyRateGreenAAA + (((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)))) / 2) - totalAmountOfExpensesAndCosts);
                                    yellowProfitPercentage = (yellowProfitAmount / ((monthlyRateGreenAAA + (((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)))) / 2)) * 100;
                                }
                                TempData["monthlyRateYellowAAA"] = "$" + ((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)).ToString("#,##0.00") + " - $" + (clientRateAndConsultantAmount.ClientRateMaximumAmount * ((decimal)globalConfiguration.NumLaborDaysInMonth) * 8).ToString("#,##0.00");
                                TempData["hourRateGreenAAA"] = "$" + hourRateGreenAAA.ToString("#,##0.00");
                                TempData["hourRateYellowAAA"] = "$" + (clientRateAndConsultantAmount.ClientRateMaximumAmount - 3).ToString("#,##0.00") + " - " + clientRateAndConsultantAmount.ClientRateMaximumAmount.ToString("#,##0.00");
                            }
                            else
                            {
                                TempData["monthlyRateYellowAAA"] = "$" + monthlyRateYellowAAA.ToString("#,##0.00") + " - $" + (monthlyRateGreenAAA - 1).ToString("#,##0.00");
                                TempData["hourRateGreenAAA"] = "$" + hourRateGreenAAA.ToString("#,##0.00");
                                TempData["hourRateYellowAAA"] = "$" + hourRateYellowAAA.ToString("#,##0.00") + " - $" + (hourRateGreenAAA - 1).ToString("#,##0.00");

                                greenProfitAmount = (monthlyRateGreenAAA - totalAmountOfExpensesAndCosts);
                                greenProfitPercentage = (greenProfitAmount / monthlyRateGreenAAA) * 100;
                                yellowProfitAmount = ((((monthlyRateGreenAAA - 1) + monthlyRateYellowAAA) / 2) - totalAmountOfExpensesAndCosts);
                                yellowProfitPercentage = (yellowProfitAmount / (((monthlyRateGreenAAA - 1) + monthlyRateYellowAAA) / 2)) * 100;
                            }
                            TempData["monthlyRateGreenAAA"] = "$" + monthlyRateGreenAAA.ToString("#,##0.00");
                            TempData["averageYellowProfitAAA"] = "$" + yellowProfitAmount.ToString("#,##0.00");
                            TempData["greenProfitAAA"] = "$" + greenProfitAmount.ToString("#,##0.00");
                            TempData["greenProfitPercentageAAA"] = greenProfitPercentage.ToString("#,##0.00") + "%";
                            TempData["yellowProfitPercentageAAA"] = yellowProfitPercentage.ToString("#,##0.00") + "%";

                        }
                        if (client.ClientClass == "B")
                        {
                            Decimal monthlyRateGreenAA = (totalAmountOfExpensesAndCosts / (100 - (decimal)globalConfiguration.ProfitGreenClientAA)) * 100;
                            Decimal monthlyRateYellowAA = (totalAmountOfExpensesAndCosts / (100 - (decimal)globalConfiguration.ProfitYellowClientAA)) * 100;
                            Decimal hourRateGreenAA = monthlyRateGreenAA / ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                            Decimal hourRateYellowAA = monthlyRateYellowAA / ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                            maxProfitSetPercentage = (decimal)globalConfiguration.ProfitGreenClientAA;
                            if (hourRateGreenAA > clientRateAndConsultantAmount.ClientRateMaximumAmount)
                            {
                                hourRateGreenAA = clientRateAndConsultantAmount.ClientRateMaximumAmount;
                                monthlyRateGreenAA = (hourRateGreenAA * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8));
                                Decimal totalAmountPayToConsultant = 0;
                                if (((monthlyRateGreenAA - totalAmountOfExpensesAndCosts) / monthlyRateGreenAA) < ((decimal)globalConfiguration.MinimumGlobalProfit) / 100)
                                {
                                    totalAmountPayToConsultant = (decimal)monthlyRateGreenAA - (((decimal)monthlyRateGreenAA * ((decimal)globalConfiguration.MinimumGlobalProfit) / 100) + (subtotalExpenses + (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease) / 100)));
                                    Decimal numHoursInMonth = (decimal)globalConfiguration.NumLaborDaysInMonth * 8;
                                    Decimal numHolidaysHours = (decimal)daysYear * 8;
                                    Decimal numVacationHours = (decimal)vacationDays * 8;
                                    Decimal hourPriceToConsultant = (totalAmountPayToConsultant / (numHoursInMonth + (numHolidaysHours / 12) + (numVacationHours / 12)));

                                    recommendedAmountToPayToConsultant = hourPriceToConsultant * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                                    recommendedAmountHolidaysToConsultant = hourPriceToConsultant * ((decimal)daysYear * 8);
                                    recommendedAmountVacationsToConsultant = hourPriceToConsultant * ((decimal)vacationDays * 8);
                                    subTotalMonthlyAmountPayToConsultant = recommendedAmountToPayToConsultant + (recommendedAmountHolidaysToConsultant / 12) + (recommendedAmountVacationsToConsultant / 12);
                                    totalAmountOfExpensesAndCosts = subTotalMonthlyAmountPayToConsultant + subtotalExpenses + (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease / 100));
                                    isProfitLessThanConfig = true;

                                    greenProfitAmount = (monthlyRateGreenAA - totalAmountPayToConsultant - subtotalExpenses - (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease / 100)));
                                    greenProfitPercentage = ((greenProfitAmount / monthlyRateGreenAA) * 100);
                                    yellowProfitAmount = (((monthlyRateGreenAA + (((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)))) / 2) - totalAmountPayToConsultant - subtotalExpenses - (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease / 100)));
                                    yellowProfitPercentage = (yellowProfitAmount / ((monthlyRateGreenAA + (((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)))) / 2)) * 100;
                                }
                                else
                                {
                                    greenProfitAmount = (monthlyRateGreenAA - totalAmountOfExpensesAndCosts);
                                    greenProfitPercentage = (greenProfitAmount / monthlyRateGreenAA) * 100;
                                    yellowProfitAmount = (((monthlyRateGreenAA + (((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)))) / 2) - totalAmountOfExpensesAndCosts);
                                    yellowProfitPercentage = (yellowProfitAmount / ((monthlyRateGreenAA + (((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)))) / 2)) * 100;
                                }
                                TempData["monthlyRateYellowAA"] = "$" + ((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)).ToString("#,##0.00") + " - $" + (clientRateAndConsultantAmount.ClientRateMaximumAmount * ((decimal)globalConfiguration.NumLaborDaysInMonth) * 8).ToString("#,##0.00");
                                TempData["hourRateGreenAA"] = "$" + hourRateGreenAA.ToString("#,##0.00");
                                TempData["hourRateYellowAA"] = "$" + (clientRateAndConsultantAmount.ClientRateMaximumAmount - 3).ToString("#,##0.00") + " - " + clientRateAndConsultantAmount.ClientRateMaximumAmount.ToString("#,##0.00");
                            }
                            else
                            {
                                TempData["monthlyRateYellowAA"] = "$" + monthlyRateYellowAA.ToString("#,##0.00") + " - $" + (monthlyRateGreenAA - 1).ToString("#,##0.00");
                                TempData["hourRateGreenAA"] = "$" + hourRateGreenAA.ToString("#,##0.00");
                                TempData["hourRateYellowAA"] = "$" + hourRateYellowAA.ToString("#,##0.00") + " - $" + (hourRateGreenAA - 1).ToString("#,##0.00");

                                greenProfitAmount = (monthlyRateGreenAA - totalAmountOfExpensesAndCosts);
                                greenProfitPercentage = (greenProfitAmount / monthlyRateGreenAA) * 100;
                                yellowProfitAmount = ((((monthlyRateGreenAA - 1) + monthlyRateYellowAA) / 2) - totalAmountOfExpensesAndCosts);
                                yellowProfitPercentage = (yellowProfitAmount / (((monthlyRateGreenAA - 1) + monthlyRateYellowAA) / 2)) * 100;

                            }
                            TempData["monthlyRateGreenAA"] = "$" + monthlyRateGreenAA.ToString("#,##0.00");
                            TempData["averageYellowProfitAA"] = "$" + yellowProfitAmount.ToString("#,##0.00");
                            TempData["greenProfitAA"] = "$" + greenProfitAmount.ToString("#,##0.00");
                            TempData["greenProfitPercentageAA"] = greenProfitPercentage.ToString("#,##0.00") + "%";
                            TempData["yellowProfitPercentageAA"] = yellowProfitPercentage.ToString("#,##0.00") + "%";
                        }
                        if (client.ClientClass == "C")
                        {
                            Decimal monthlyRateGreenPartner = (totalAmountOfExpensesAndCosts / (100 - (decimal)globalConfiguration.ProfitGreenPartner)) * 100;
                            Decimal monthlyRateYellowPartner = (totalAmountOfExpensesAndCosts / (100 - (decimal)globalConfiguration.ProfitYellowPartner)) * 100;
                            Decimal hourRateGreenPartner = monthlyRateGreenPartner / ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                            Decimal hourRateYellowPartner = monthlyRateYellowPartner / ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                            maxProfitSetPercentage = (decimal)globalConfiguration.ProfitGreenPartner;
                            if (hourRateGreenPartner > clientRateAndConsultantAmount.ClientRateMaximumAmount)
                            {
                                hourRateGreenPartner = clientRateAndConsultantAmount.ClientRateMaximumAmount;
                                monthlyRateGreenPartner = (hourRateGreenPartner * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8));
                                Decimal totalAmountPayToConsultant = 0;
                                if (((monthlyRateGreenPartner - totalAmountOfExpensesAndCosts) / monthlyRateGreenPartner) < ((decimal)globalConfiguration.MinimumGlobalProfit) / 100)
                                {
                                    totalAmountPayToConsultant = (decimal)monthlyRateGreenPartner - (((decimal)monthlyRateGreenPartner * ((decimal)globalConfiguration.MinimumGlobalProfit) / 100) + (subtotalExpenses + (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease) / 100)));
                                    Decimal numHoursInMonth = (decimal)globalConfiguration.NumLaborDaysInMonth * 8;
                                    Decimal numHolidaysHours = (decimal)daysYear * 8;
                                    Decimal numVacationHours = (decimal)vacationDays * 8;
                                    Decimal hourPriceToConsultant = (totalAmountPayToConsultant / (numHoursInMonth + (numHolidaysHours / 12) + (numVacationHours / 12)));

                                    recommendedAmountToPayToConsultant = hourPriceToConsultant * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                                    recommendedAmountHolidaysToConsultant = hourPriceToConsultant * ((decimal)daysYear * 8);
                                    recommendedAmountVacationsToConsultant = hourPriceToConsultant * ((decimal)vacationDays * 8);
                                    subTotalMonthlyAmountPayToConsultant = recommendedAmountToPayToConsultant + (recommendedAmountHolidaysToConsultant / 12) + (recommendedAmountVacationsToConsultant / 12);
                                    totalAmountOfExpensesAndCosts = subTotalMonthlyAmountPayToConsultant + subtotalExpenses + (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease / 100));
                                    isProfitLessThanConfig = true;

                                    greenProfitAmount = (monthlyRateGreenPartner - totalAmountPayToConsultant - subtotalExpenses - (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease / 100)));
                                    greenProfitPercentage = ((greenProfitAmount / monthlyRateGreenPartner) * 100);
                                    yellowProfitAmount = (((monthlyRateGreenPartner + (((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)))) / 2) - totalAmountPayToConsultant - subtotalExpenses - (subtotalExpenses * ((decimal)globalConfiguration.AdditionalGlobalIncrease / 100)));
                                    yellowProfitPercentage = (yellowProfitAmount / ((monthlyRateGreenPartner + (((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)))) / 2)) * 100;
                                }
                                else
                                {
                                    greenProfitAmount = (monthlyRateGreenPartner - totalAmountOfExpensesAndCosts);
                                    greenProfitPercentage = (greenProfitAmount / monthlyRateGreenPartner) * 100;
                                    yellowProfitAmount = (((monthlyRateGreenPartner + (((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)))) / 2) - totalAmountOfExpensesAndCosts);
                                    yellowProfitPercentage = (yellowProfitAmount / ((monthlyRateGreenPartner + (((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)))) / 2)) * 100;
                                }
                                TempData["monthlyRateYellowPartner"] = "$" + ((clientRateAndConsultantAmount.ClientRateMaximumAmount - 3) * ((decimal)globalConfiguration.NumLaborDaysInMonth * 8)).ToString("#,##0.00") + " - $" + (clientRateAndConsultantAmount.ClientRateMaximumAmount * ((decimal)globalConfiguration.NumLaborDaysInMonth) * 8).ToString("#,##0.00");
                                TempData["hourRateGreenPartner"] = "$" + hourRateGreenPartner.ToString("#,##0.00");
                                TempData["hourRateYellowPartner"] = "$" + (clientRateAndConsultantAmount.ClientRateMaximumAmount - 3).ToString("#,##0.00") + " - " + clientRateAndConsultantAmount.ClientRateMaximumAmount.ToString("#,##0.00");
                            }
                            else
                            {
                                TempData["monthlyRateYellowPartner"] = "$" + monthlyRateYellowPartner.ToString("#,##0.00") + " - $" + (monthlyRateGreenPartner - 1).ToString("#,##0.00");
                                TempData["hourRateGreenPartner"] = "$" + hourRateGreenPartner.ToString("#,##0.00");
                                TempData["hourRateYellowPartner"] = "$" + hourRateYellowPartner.ToString("#,##0.00") + " - $" + (hourRateGreenPartner - 1).ToString("#,##0.00");

                                greenProfitAmount = (monthlyRateGreenPartner - totalAmountOfExpensesAndCosts);
                                greenProfitPercentage = (greenProfitAmount / monthlyRateGreenPartner) * 100;
                                yellowProfitAmount = ((((monthlyRateGreenPartner - 1) + monthlyRateYellowPartner) / 2) - totalAmountOfExpensesAndCosts);
                                yellowProfitPercentage = (yellowProfitAmount / (((monthlyRateGreenPartner - 1) + monthlyRateYellowPartner) / 2)) * 100;
                            }
                            TempData["monthlyRateGreenPartner"] = "$" + monthlyRateGreenPartner.ToString("#,##0.00");
                            TempData["averageYellowProfitPartner"] = "$" + yellowProfitAmount.ToString("#,##0.00");
                            TempData["greenProfitPartner"] = "$" + greenProfitAmount.ToString("#,##0.00");
                            TempData["greenProfitPercentagePartner"] = greenProfitPercentage.ToString("#,##0.00") + "%";
                            TempData["yellowProfitPercentagePartner"] = yellowProfitPercentage.ToString("#,##0.00") + "%";
                        }
                        if (isProfitLessThanConfig)
                        {
                            TempData["recommendedAmountToPayToConsultant"] = recommendedAmountToPayToConsultant.ToString("#,##0.00");
                            TempData["recommendedAmountHolidaysToConsultant"] = recommendedAmountHolidaysToConsultant.ToString("#,##0.00");
                            if (model.VacationDays > 0)
                            {
                                TempData["recommendedAmountVacationsToConsultant"] = recommendedAmountVacationsToConsultant.ToString("#,##0.00");
                            }
                            TempData["messageProfitLessThanConfig"] = "Valla!, Parece que ingresaste un monto muy alto para el rol y el nivel que tiene el consultor, " +
                                "te recomiendo un monto menor o igual a: $" + recommendedAmountToPayToConsultant.ToString("#,##0.00") + ", así pagarás anualmente un monto de: $" +
                                recommendedAmountHolidaysToConsultant.ToString("#,##0.00") + " para Holidays y un monto anual de: $" + recommendedAmountVacationsToConsultant.ToString("#,##0.00") + " para días de vacaciones.";
                        }
                    }

                    if (userIsMasterOrAdmin > 0)
                    {
                        TempData["subtotalExpenses"] = subtotalExpenses.ToString("#,##0.00");
                        TempData["totalCosts"] = subTotalMonthlyAmountPayToConsultant.ToString("#,##0.00");
                        TempData["appliedAmountGlobalIncrease"] = "(" + globalConfiguration.AdditionalGlobalIncrease + "%): $" + appliedAmountGlobalIncrease.ToString("#,##0.00");
                        TempData["totalAmountOfExpensesAndCosts"] = totalAmountOfExpensesAndCosts.ToString("#,##0.00");


                        if (appliedAmountGlobalIncrease > 0)
                        {
                            expensesCostsDistributionList.Add(new CalculatorExpensesCostsDistribution
                            {
                                AccountingAccountId = 0,
                                AccountingAccountCode = "NO APLICA",
                                AccountingAccountName = "Monto Aumento Global Aplicado",
                                Amount = appliedAmountGlobalIncrease,
                                CostCenterName = "NO ASIGNADO A CENTRO DE COSTO",
                                increasePercentage = 0,
                                increaseAmount = 0,
                                CompanyId = "OCE"
                            });
                        }

                        expensesCostsDistributionList.Add(new CalculatorExpensesCostsDistribution
                        {
                            AccountingAccountId = 0,
                            AccountingAccountCode = "5-01-01-000-000",
                            AccountingAccountName = "Horas de recursos",
                            Amount = consultantMonthlyPayment,
                            CostCenterName = "NO ASIGNADO A CENTRO DE COSTO",
                            increasePercentage = 0,
                            increaseAmount = 0,
                            CompanyId = "OCE"
                        });
                        if (consultantVacationsAmount > 0)
                        {
                            expensesCostsDistributionList.Add(new CalculatorExpensesCostsDistribution
                            {
                                AccountingAccountId = 0,
                                AccountingAccountCode = "5-01-02-000-000",
                                AccountingAccountName = "Vacaciones de recursos",
                                Amount = (consultantVacationsAmount / 12),
                                CostCenterName = "NO ASIGNADO A CENTRO DE COSTO",
                                increasePercentage = 0,
                                increaseAmount = 0,
                                CompanyId = "OCE"
                            });
                        }
                        expensesCostsDistributionList.Add(new CalculatorExpensesCostsDistribution
                        {
                            AccountingAccountId = 0,
                            AccountingAccountCode = "5-01-06-000-000",
                            AccountingAccountName = "Días Feriados de Recursos",
                            Amount = (consultantHolidaysAmount / 12),
                            CostCenterName = "NO ASIGNADO A CENTRO DE COSTO",
                            increasePercentage = 0,
                            increaseAmount = 0,
                            CompanyId = "OCE"
                        });
                        expensesCostsDistributionList.Sort((p, q) => p.AccountingAccountCode.CompareTo(q.AccountingAccountCode));
                    }
                    else
                    {
                        expensesCostsDistributionList = null;
                    }
                    // SAVE SEARCH HISTORY
                    CalculatorSearchHistory searchHistory = new()
                    {
                        SearchDate = DateTime.Now,
                        SearchByUserId = claim.Value
                    };
                    _unitOfWork.CalculatorSearchHistory.Add(searchHistory);
                    _unitOfWork.Save();

                    //MODEL TO RETURN
                    CalculatorVM cvm = new()
                    {
                        Client = model.Client,
                        ConsultantRoleId = model.ConsultantRoleId,
                        ConsultantQualityLevelId = model.ConsultantQualityLevelId,
                        GreenPercentageInResults = greenProfitPercentage,
                        MinProfitSetPercentage = (decimal)globalConfiguration.MinimumGlobalProfit,
                        MaxProfitSetPercentage = maxProfitSetPercentage,
                        ClientList = model.ClientList,
                        ConsultantRoleList = model.ConsultantRoleList,
                        ConsultantQualityLevelList = model.ConsultantQualityLevelList,
                        CalculatorCostCenterUserConfigurationVM = model.CalculatorCostCenterUserConfigurationVM,
                        CalculatorExpensesCostsDistribution = expensesCostsDistributionList
                    };

                    TempData["result"] = "success";

                    return View("Index", cvm);
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            else
            {
                //MODEL TO RETURN
                CalculatorVM cvm = new()
                {
                    Client = model.Client,
                    ConsultantRoleId = model.ConsultantRoleId,
                    ConsultantQualityLevelId = model.ConsultantQualityLevelId,
                    ClientList = model.ClientList,
                    ConsultantRoleList = model.ConsultantRoleList,
                    ConsultantQualityLevelList = model.ConsultantQualityLevelList,
                    CalculatorCostCenterUserConfigurationVM = model.CalculatorCostCenterUserConfigurationVM
                };
                return View("Index", cvm);
            }
        }
    }
}
