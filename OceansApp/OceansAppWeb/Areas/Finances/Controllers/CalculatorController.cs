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
                    Active = true
                };
                costCenterUserList.Add(costCenterUserObj);
            }

            var clients = _unitOfWork.Client.GetAll(x => x.ClientCategory == "EXT" && x.IsActive == "S").Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.IdClient
            }); ;

            CalculatorVM cvm = new()
            {
                ClientList = clients.ToList(),
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
                                            IdAccountingAccount = accountingAccount.AccountingAccountCode,
                                            AccountingAccountName = accountingAccount.AccountingAccountName,
                                            Amount = (((decimal)amountByCostCenter + (decimal)totalAmountByCostCenterAfterPercentage) / (decimal)numMonths) / (decimal)globalConfiguration.PeopleNumber,
                                            CostCenterName = costCenter.Description,
                                            increasePercentage = percentageIncrease,
                                            increaseAmount = ((decimal)totalAmountByCostCenterAfterPercentage / (decimal)numMonths) / (decimal)globalConfiguration.PeopleNumber
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
                                                IdAccountingAccount = accountingAccount.AccountingAccountCode,
                                                AccountingAccountName = accountingAccount.AccountingAccountName,
                                                Amount = (((decimal)amountByCostCenter + (decimal)totalAmountByCostCenterAfterPercentage) / (decimal)numMonths) / (decimal)globalConfiguration.PeopleNumber,
                                                CostCenterName = costCenter.Description,
                                                increasePercentage = percentageIncrease,
                                                increaseAmount = ((decimal)totalAmountByCostCenterAfterPercentage / (decimal)numMonths) / (decimal)globalConfiguration.PeopleNumber
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
                                    IdAccountingAccount = accountingAccount.AccountingAccountCode,
                                    AccountingAccountName = accountingAccount.AccountingAccountName,
                                    Amount = ((decimal)amount / (decimal)numMonths) / (decimal)globalConfiguration.PeopleNumber,
                                    CostCenterName = "NO ASIGNADO A CENTRO DE COSTO",
                                    increasePercentage = 0,
                                    increaseAmount = 0
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

                    var client = _unitOfWork.Client.GetFirstOrDefault(x => x.IdClient == model.Client);
                    if (client != null)
                    {
                        if (client.ClientClass == "A")
                        {
                            Decimal monthlyRateGreenAAA = (totalAmountOfExpensesAndCosts / (100 - (decimal)globalConfiguration.ProfitGreenClientAAA)) * 100;
                            Decimal monthlyRateYellowAAA = (totalAmountOfExpensesAndCosts / (100 - (decimal)globalConfiguration.ProfitYellowClientAAA)) * 100;
                            Decimal hourRateGreenAAA = monthlyRateGreenAAA / ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                            Decimal hourRateYellowAAA = monthlyRateYellowAAA / ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                            TempData["monthlyRateGreenAAA"] = "$" + monthlyRateGreenAAA.ToString("#,##0.00");
                            TempData["monthlyRateYellowAAA"] = "$" + (monthlyRateGreenAAA - 1).ToString("#,##0.00") + " - $" + monthlyRateYellowAAA.ToString("#,##0.00");
                            TempData["hourRateGreenAAA"] = "$" + hourRateGreenAAA.ToString("#,##0.00");
                            TempData["hourRateYellowAAA"] = "$" + (hourRateGreenAAA - 1).ToString("#,##0.00") + " - $" + hourRateYellowAAA.ToString("#,##0.00");
                            if (userIsMasterOrAdmin > 0)
                            {
                                TempData["greenProfitAAA"] = "$" + (monthlyRateGreenAAA - totalAmountOfExpensesAndCosts).ToString("#,##0.00");
                                TempData["averageYellowProfitAAA"] = "$" + ((((monthlyRateGreenAAA - 1) + monthlyRateYellowAAA) / 2) - totalAmountOfExpensesAndCosts).ToString("#,##0.00");
                            }
                        }
                        if (client.ClientClass == "B")
                        {
                            Decimal monthlyRateGreenAA = (totalAmountOfExpensesAndCosts / (100 - (decimal)globalConfiguration.ProfitGreenClientAA)) * 100;
                            Decimal monthlyRateYellowAA = (totalAmountOfExpensesAndCosts / (100 - (decimal)globalConfiguration.ProfitYellowClientAA)) * 100;
                            Decimal hourRateGreenAA = monthlyRateGreenAA / ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                            Decimal hourRateYellowAA = monthlyRateYellowAA / ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                            TempData["monthlyRateGreenAA"] = "$" + monthlyRateGreenAA.ToString("#,##0.00");
                            TempData["monthlyRateYellowAA"] = "$" + (monthlyRateGreenAA - 1).ToString("#,##0.00") + " - $" + monthlyRateYellowAA.ToString("#,##0.00");
                            TempData["hourRateGreenAA"] = "$" + hourRateGreenAA.ToString("#,##0.00");
                            TempData["hourRateYellowAA"] = "$" + (hourRateGreenAA - 1).ToString("#,##0.00") + " - $" + hourRateYellowAA.ToString("#,##0.00");
                            if (userIsMasterOrAdmin > 0)
                            {
                                TempData["greenProfitAA"] = "$" + (monthlyRateGreenAA - totalAmountOfExpensesAndCosts).ToString("#,##0.00");
                                TempData["averageYellowProfitAA"] = "$" + ((((monthlyRateGreenAA - 1) + monthlyRateYellowAA) / 2) - totalAmountOfExpensesAndCosts).ToString("#,##0.00");
                            }
                        }
                        if (client.ClientClass == "C")
                        {
                            Decimal monthlyRateGreenPartner = (totalAmountOfExpensesAndCosts / (100 - (decimal)globalConfiguration.ProfitGreenPartner)) * 100;
                            Decimal monthlyRateYellowPartner = (totalAmountOfExpensesAndCosts / (100 - (decimal)globalConfiguration.ProfitYellowPartner)) * 100;
                            Decimal hourRateGreenPartner = monthlyRateGreenPartner / ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                            Decimal hourRateYellowPartner = monthlyRateYellowPartner / ((decimal)globalConfiguration.NumLaborDaysInMonth * 8);
                            TempData["monthlyRateGreenPartner"] = "$" + monthlyRateGreenPartner.ToString("#,##0.00");
                            TempData["monthlyRateYellowPartner"] = "$" + (monthlyRateGreenPartner - 1).ToString("#,##0.00") + " - $" + monthlyRateYellowPartner.ToString("#,##0.00");
                            TempData["hourRateGreenPartner"] = "$" + hourRateGreenPartner.ToString("#,##0.00");
                            TempData["hourRateYellowPartner"] = "$" + (hourRateGreenPartner - 1).ToString("#,##0.00") + " - $" + hourRateYellowPartner.ToString("#,##0.00");
                            if (userIsMasterOrAdmin > 0)
                            {
                                TempData["greenProfitPartner"] = "$" + (monthlyRateGreenPartner - totalAmountOfExpensesAndCosts).ToString("#,##0.00");
                                TempData["averageYellowProfitPartner"] = "$" + ((((monthlyRateGreenPartner - 1) + monthlyRateYellowPartner) / 2) - totalAmountOfExpensesAndCosts).ToString("#,##0.00");
                            }
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
                                IdAccountingAccount = "NO APLICA",
                                AccountingAccountName = "Monto Aumento Global Aplicado",
                                Amount = appliedAmountGlobalIncrease,
                                CostCenterName = "NO ASIGNADO A CENTRO DE COSTO",
                                increasePercentage = 0,
                                increaseAmount = 0
                            });
                        }

                        expensesCostsDistributionList.Add(new CalculatorExpensesCostsDistribution
                        {
                            IdAccountingAccount = "5-01-01-000-000",
                            AccountingAccountName = "Horas de recursos",
                            Amount = consultantMonthlyPayment,
                            CostCenterName = "NO ASIGNADO A CENTRO DE COSTO",
                            increasePercentage = 0,
                            increaseAmount = 0
                        });
                        if (consultantVacationsAmount > 0)
                        {
                            expensesCostsDistributionList.Add(new CalculatorExpensesCostsDistribution
                            {
                                IdAccountingAccount = "5-01-02-000-000",
                                AccountingAccountName = "Vacaciones de recursos",
                                Amount = (consultantVacationsAmount / 12),
                                CostCenterName = "NO ASIGNADO A CENTRO DE COSTO",
                                increasePercentage = 0,
                                increaseAmount = 0
                            });
                        }
                        expensesCostsDistributionList.Add(new CalculatorExpensesCostsDistribution
                        {
                            IdAccountingAccount = "5-01-06-000-000",
                            AccountingAccountName = "Días Feriados de Recursos",
                            Amount = (consultantHolidaysAmount / 12),
                            CostCenterName = "NO ASIGNADO A CENTRO DE COSTO",
                            increasePercentage = 0,
                            increaseAmount = 0
                        });
                        expensesCostsDistributionList.Sort((p, q) => p.IdAccountingAccount.CompareTo(q.IdAccountingAccount));
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
                        ClientList = model.ClientList,
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
                    ClientList = model.ClientList,
                    CalculatorCostCenterUserConfigurationVM = model.CalculatorCostCenterUserConfigurationVM
                };
                return View("Index", cvm);
            }
        }
    }
}
