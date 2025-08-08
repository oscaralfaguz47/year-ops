using OceansApp.Models.Models;
using OceansApp.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OceansApp.DataAccess.Data;
using OceansApp.Utility.ConstantData.Claims.AdminCenter;
using OceansApp.Utility.ConstantData.Claims.Finances;
using OceansApp.Utility.ConstantData.Claims.General;
using OceansApp.Utility.ConstantData.Claims.TrackingTool;
using OceansApp.Utility.ConstantData.Claims.AccountManagement;
using OceansApp.Utility.ConstantData.Claims.Recruiting;
using OceansApp.Models.ViewModels.ConsultantPositions;

namespace OceansApp.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public IConfiguration _config { get; }

        public DbInitializer(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if ((await _db.Database.GetPendingMigrationsAsync()).Any())
                {
                    await _db.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error during executing pending migrations:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                throw;
            }

            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // ----------------- USER CATEGORIES --------------------------------

                    List<ApplicationUserCategory> userCategoriesList = new List<ApplicationUserCategory>
                {
                    new ApplicationUserCategory { Name = "Administrative" },
                    new ApplicationUserCategory { Name = "Consultant" },
                    new ApplicationUserCategory { Name = "External User" }
                };
                    foreach (var userCategory in userCategoriesList)
                    {
                        if (await _db.UserCategories.FirstOrDefaultAsync(x => x.Name == userCategory.Name) == null)
                        {
                            await _db.UserCategories.AddAsync(userCategory);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- COMPANIES --------------------------------

                    List<Company> companiesList = new List<Company>
                {
                    new Company { CompanyId = "OCE", Name = "Oceans Consulting Firm, S.A" },
                    new Company { CompanyId = "LLC", Name = "OCE LLC" }
                };

                    foreach (var company in companiesList)
                    {
                        var existingCompany = await _db.COMPANIES.FirstOrDefaultAsync(x => x.Name == company.Name);
                        if (existingCompany == null)
                        {
                            Company companyToCreate = new()
                            {
                                CompanyId = company.CompanyId,
                                Name = company.Name
                            };
                            await _db.COMPANIES.AddAsync(companyToCreate);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- COUNTRIES --------------------------------

                    List<Country> countriesList = new List<Country>
                {
                    new Country { IdCountry = "ARN", Name = "Argentina" },
                    new Country { IdCountry = "BLC", Name = "Belice"},
                    new Country { IdCountry = "BOL", Name = "Bolivia" },
                    new Country { IdCountry = "BRA", Name = "Brasil" },
                    new Country { IdCountry = "CAN", Name = "Canadá"},
                    new Country { IdCountry = "CHL", Name = "Chile" },
                    new Country { IdCountry = "CHN", Name = "China" },
                    new Country { IdCountry = "CNO", Name = "República Popular Democrática de Corea"},
                    new Country { IdCountry = "COL", Name = "Colombia"},
                    new Country { IdCountry = "CRI", Name = "Costa Rica" },
                    new Country { IdCountry = "CUB", Name = "Cuba" },
                    new Country { IdCountry = "DIN", Name = "Dinamarca" },
                    new Country { IdCountry = "ECU", Name = "Ecuador" },
                    new Country { IdCountry = "ESP", Name = "España"},
                    new Country { IdCountry = "FIN", Name = "Finlandia"},
                    new Country { IdCountry = "FRA", Name = "Francia"},
                    new Country { IdCountry = "GUA", Name = "Guatemala"},
                    new Country { IdCountry = "HOL", Name = "Países Bajos"},
                    new Country { IdCountry = "HON", Name = "Honduras" },
                    new Country { IdCountry = "IDI", Name = "India"},
                    new Country { IdCountry = "ISR", Name = "Israel" },
                    new Country { IdCountry = "ITA", Name = "Italia" },
                    new Country { IdCountry = "JAM", Name = "Jamaica" },
                    new Country { IdCountry = "JAP", Name = "Japón" },
                    new Country { IdCountry = "MAL", Name = "Malasia"},
                    new Country { IdCountry = "MEX", Name = "México" },
                    new Country { IdCountry = "ND", Name = "NO DEFINIDO"},
                    new Country { IdCountry = "NIC", Name = "Nicaragua" },
                    new Country { IdCountry = "NOR", Name = "Noruega" },
                    new Country { IdCountry = "NZE", Name = "Nueva Zelandia" },
                    new Country { IdCountry = "PAN", Name = "Panamá"},
                    new Country { IdCountry = "PAR", Name = "Paraguay" },
                    new Country { IdCountry = "PER", Name = "Perú" },
                    new Country { IdCountry = "POL", Name = "Polonia"},
                    new Country { IdCountry = "POR", Name = "Portugal" },
                    new Country { IdCountry = "PRI", Name = "Puerto Rico" },
                    new Country { IdCountry = "RDO", Name = "República Dominicana" },
                    new Country { IdCountry = "RFA", Name = "Alemania" },
                    new Country { IdCountry = "RUS", Name = "Federación de Rusia" },
                    new Country { IdCountry = "SAF", Name = "Sudáfrica" },
                    new Country { IdCountry = "SAL", Name = "El Salvador" },
                    new Country { IdCountry = "SIN", Name = "Singapur"},
                    new Country { IdCountry = "SUE", Name = "Suecia" },
                    new Country { IdCountry = "SUI", Name = "Suiza" },
                    new Country { IdCountry = "SUR", Name = "Suriname"},
                    new Country { IdCountry = "TRI", Name = "Trinidad y Tobago" },
                    new Country { IdCountry = "TUR", Name = "Turquía"},
                    new Country { IdCountry = "TWN", Name = "República China Taiwan" },
                    new Country { IdCountry = "URU", Name = "Uruguay"},
                    new Country { IdCountry = "USA", Name = "Estados Unidos" },
                    new Country { IdCountry = "VEN", Name = "Venezuela"},
                    new Country { IdCountry = "YUG", Name = "Yugoslavia" },
                };

                    foreach (var country in countriesList)
                    {
                        var existingCountry = await _db.COUNTRY.FirstOrDefaultAsync(x => x.IdCountry == country.IdCountry);
                        if (existingCountry == null)
                        {
                            Country countryToCreate = new()
                            {
                                IdCountry = country.IdCountry,
                                Name = country.Name,
                                CreateDate = DateTime.UtcNow
                            };
                            await _db.COUNTRY.AddAsync(countryToCreate);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- CONSULTANT BENEFITS --------------------------------

                    List<ConsultantBenefit> consultantBenefitList = new List<ConsultantBenefit>
                {
                    new ConsultantBenefit { Name = "Balance Program", Amount = 750, BenefitPeriod = "Annual" },
                    new ConsultantBenefit { Name = "Bonusly", Amount = 5000, BenefitPeriod = "Undefined" },
                    new ConsultantBenefit { Name = "Oceans Challenge", Amount = 250, BenefitPeriod = "Annual" }
                };

                    foreach (var benefit in consultantBenefitList)
                    {
                        var existingBenefit = await _db.CONSULTANT_BENEFITS.FirstOrDefaultAsync(x => x.Name == benefit.Name);
                        if (existingBenefit == null)
                        {
                            ConsultantBenefit conBenefit = new()
                            {
                                Name = benefit.Name,
                                Amount = benefit.Amount,
                                BenefitPeriod = benefit.BenefitPeriod
                            };
                            await _db.CONSULTANT_BENEFITS.AddAsync(conBenefit);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- CONSULTANT BENEFITS CATEGORIES --------------------------------

                    List<ConsultantBenefitCategory> consultantBenefitCategoriesList = new List<ConsultantBenefitCategory>();
                    var balanceProgramBenefit = await _db.CONSULTANT_BENEFITS.FirstOrDefaultAsync(x => x.Name == "Balance Program");
                    var bonuslyBenefit = await _db.CONSULTANT_BENEFITS.FirstOrDefaultAsync(x => x.Name == "Bonusly");
                    var oceansChallengeBenefit = await _db.CONSULTANT_BENEFITS.FirstOrDefaultAsync(x => x.Name == "Oceans Challenge");

                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Expert Boost ($250) (2500 Bonus.ly XP)", BenefitId = balanceProgramBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Wellness Coverage ($750)", BenefitId = balanceProgramBenefit.BenefitId });

                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Curiosity Stream 1 year ($25)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "A new gaming console ($500)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Adventure tickets ($100)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Buy a book! ($25)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Ergonomics ($150)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Gamers ($200)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Hotel or plane tickets ($240/$480/$750)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Just Cash Out ($50/$100/$200)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Lodgings ($100) ", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Movie Night ($30)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Music Lovers! ($60)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "N. Fitness Freaks ($120)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Nintendo Switch ONLINE 1 year ($40)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Out for dinner ($80)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Personal Care ($35/$70/$140)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "PlayStation Plus ($60)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Streaming Subscriptions ($20)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Tech gadgets I ($30)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Tech gadgets II ($140)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Tech gadgets III ($300)", BenefitId = bonuslyBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "UberEats voucher ($25)", BenefitId = bonuslyBenefit.BenefitId });

                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Courses (in person/online)", BenefitId = oceansChallengeBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Licenses for learning tools and work support", BenefitId = oceansChallengeBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Universities Enrollment", BenefitId = oceansChallengeBenefit.BenefitId });
                    consultantBenefitCategoriesList.Add(new ConsultantBenefitCategory { Name = "Certificates", BenefitId = oceansChallengeBenefit.BenefitId });

                    foreach (var category in consultantBenefitCategoriesList)
                    {
                        var existingCategory = await _db.CONSULTANT_BENEFIT_CATEGORIES.FirstOrDefaultAsync(x => x.Name == category.Name);
                        if (existingCategory == null)
                        {
                            ConsultantBenefitCategory conBenefitCategory = new()
                            {
                                Name = category.Name,
                                BenefitId = category.BenefitId
                            };
                            await _db.CONSULTANT_BENEFIT_CATEGORIES.AddAsync(conBenefitCategory);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- DEFAULT COST CENTERS --------------------------------

                    List<CostCenter> costCentersList = new List<CostCenter>
                {
                    new CostCenter {
        CostCenterCode = "10-02-04",
        Description = "Area People and Culture",
        Detail = "Gastos especificos que abarcan el Área de People and Culture, como: Beneficios, regalías y presentes a personal, actividades sociales, etc.",
        AcceptData = "S", CompanyId = "OCE"},
                    new CostCenter {
        CostCenterCode = "10-02-04",
        Description = "Area People and Culture",
        Detail = null,
        AcceptData = "S", CompanyId = "LLC"},
                    new CostCenter {
        CostCenterCode = "50-01-00",
        Description = "Area Finanzas",
        Detail = "Gastos especificos que abarcan el Área de Finanzas, como: Herramientas contables, Servicios contables, Licencias en esta área, etc.",
        AcceptData = "S", CompanyId = "OCE"},
                    new CostCenter {
        CostCenterCode = "50-01-00",
        Description = "Area Finanzas",
        Detail = null,
        AcceptData = "S", CompanyId = "LLC"},
                    new CostCenter {
        CostCenterCode = "30-01-01",
        Description = "Area Reclutamiento",
        Detail = "Gastos especificos que abarcan el Área de Recursos Humanos, como: Pagos a reclutadoras, Pagos a entrevistadores, Licencias en esta área, etc.",
        AcceptData = "S", CompanyId = "OCE"},
                    new CostCenter {
        CostCenterCode = "30-01-01",
        Description = "Area Reclutamiento",
        Detail = null,
        AcceptData = "S", CompanyId = "LLC"},
                    new CostCenter {
        CostCenterCode = "10-02-02",
        Description = "Area ejecutivos de cuentas",
        Detail = "Gastos especificos que abarcan el Área de Ejecutivos de Cuentas, como: Pagos a ejecutivos de cuentas, Licencias en esta área, etc.",
        AcceptData = "S", CompanyId = "OCE"},
                    new CostCenter {
        CostCenterCode = "10-02-02",
        Description = "Area ejecutivos de cuentas",
        Detail = null,
        AcceptData = "S", CompanyId = "LLC"},
                    new CostCenter {
        CostCenterCode = "40-01-01",
        Description = "Operaciones Recursos",
        Detail = "Gastos en los que se incurre para que los Desarrolladores y QAs puedan trabajar, como: tracking tools, envíos de computadoras, etc.",
        AcceptData = "S", CompanyId = "OCE"},
                    new CostCenter {
        CostCenterCode = "40-01-01",
        Description = "Operaciones Recursos",
        Detail = null,
        AcceptData = "S", CompanyId = "LLC"}
                };

                    foreach (var costCenter in costCentersList)
                    {
                        var existingCostCenter = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == costCenter.CostCenterCode && x.CompanyId == costCenter.CompanyId);
                        if (existingCostCenter == null)
                        {
                            CostCenter costCenterToCreate = new()
                            {
                                CostCenterCode = costCenter.CostCenterCode,
                                Description = costCenter.Description,
                                Detail = costCenter.Detail,
                                AcceptData = costCenter.AcceptData,
                                CreateDate = DateTime.UtcNow,
                                CompanyId = costCenter.CompanyId
                            };
                            await _db.COST_CENTER.AddAsync(costCenterToCreate);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- DEFAULT ACCOUNTING ACCOUNTS --------------------------------

                    List<AccountingAccount> accountingAccountsList = new List<AccountingAccount>
                {
                      new AccountingAccount {
        Description = "Reserva para beneficios (Balance Program)",
        AccountingAccountType = "B",
        DetailedType = "T",
        Balance = "A",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "3-02-01-000-000",
        CompanyId = "OCE",
        DescriptionIFRS = "Reserva para beneficios (Balance Program)"},
                      new AccountingAccount {
        Description = "Reserva para beneficios (Bonusly)",
        AccountingAccountType = "B",
        DetailedType = "T",
        Balance = "A",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "3-02-02-000-000",
        CompanyId = "OCE",
        DescriptionIFRS = "Reserva para beneficios (Bonusly)"},
                      new AccountingAccount {
        Description = "Cursos / Capacitaciones (Oceans Challenge)",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "6-01-03-005-000",
        CompanyId = "OCE",
        DescriptionIFRS = "Cursos / Capacitaciones (Oceans Challenge)"},
                      new AccountingAccount {
        Description = "Administrative expenses for OCE",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "N",
        AccountingAccountCode = "6-01-04-013-0000",
        CompanyId = "LLC",
        DescriptionIFRS = "Gastos administrativos para OCE"},
                      new AccountingAccount {
        Description = "Pago a recursos Administrativos",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "6-01-01-001-000",
        CompanyId = "OCE",
        DescriptionIFRS = "Horas y salarios a personal administrativo"},
                      new AccountingAccount {
        Description = "Payment to administrative consultants",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "6-01-01-001-0000",
        CompanyId = "LLC",
        DescriptionIFRS = "Pago a recursos Administrativos"},
                      new AccountingAccount {
        Description = "Días Feriados Administrativos",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "6-01-01-002-000",
        CompanyId = "OCE",
        DescriptionIFRS = "Feriados Administrativos"},
                      new AccountingAccount {
        Description = "Administrative consultant holidays",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "6-01-01-002-0000",
        CompanyId = "LLC",
        DescriptionIFRS = "Días Feriados Administrativos"},
                      new AccountingAccount {
        Description = "Entrevistas con Prospectos",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "6-01-01-005-000",
        CompanyId = "OCE",
        DescriptionIFRS = "Horas adicionales pagadas a personas que participan en entrevistas"},
                      new AccountingAccount {
        Description = "Pagos a ejecutivos de cuenta",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "6-01-01-007-000",
        CompanyId = "OCE",
        DescriptionIFRS = "Pagos a ejecutivos de cuenta"},
                      new AccountingAccount {
        Description = "Horas de recursos",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "5-01-01-000-000",
        CompanyId = "OCE",
        DescriptionIFRS = "Horas de recursos"},
                      new AccountingAccount {
        Description = "Consultant hours",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "5-01-01-000-0000",
        CompanyId = "LLC",
        DescriptionIFRS = "Horas de recursos"},
                      new AccountingAccount {
        Description = "Días Feriados de Recursos",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "5-01-06-000-000",
        CompanyId = "OCE",
        DescriptionIFRS = "Días Feriados de Recursos"},
                      new AccountingAccount {
        Description = "Consultant holidays",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "5-01-03-000-0000",
        CompanyId = "LLC",
        DescriptionIFRS = "Días Feriados de Recursos"},
                      new AccountingAccount {
        Description = "Tarifa de guardia (On Call)",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "S",
        AccountingAccountCode = "5-01-16-000-000",
        CompanyId = "OCE",
        DescriptionIFRS = "Tarifa de guardia (On Call)"},
                      new AccountingAccount {
        Description = "On Call flate rate",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "N",
        AccountingAccountCode = "5-01-08-000-0000",
        CompanyId = "LLC",
        DescriptionIFRS = "Tarifa de guardia (On call)"},
                      new AccountingAccount {
        Description = "Horas en On Call trabajadas",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "N",
        AccountingAccountCode = "5-01-22-000-000",
        CompanyId = "OCE",
        DescriptionIFRS = "Horas en On Call trabajadas"},
                      new AccountingAccount {
        Description = "On Call hours worked",
        AccountingAccountType = "E",
        DetailedType = "G",
        Balance = "D",
        AcceptData = "S",
        UseCostCenter = "S",
        UseThird = "N",
        AccountingAccountCode = "5-01-09-000-0000",
        CompanyId = "LLC",
        DescriptionIFRS = "Horas trabajadas en On Call"}
                };

                    foreach (var accountingAccount in accountingAccountsList)
                    {
                        var existingAccountingAccount = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == accountingAccount.AccountingAccountCode &&
                        x.CompanyId == accountingAccount.CompanyId);
                        if (existingAccountingAccount == null)
                        {
                            AccountingAccount accountingAccountToCreate = new()
                            {
                                Description = accountingAccount.Description,
                                AccountingAccountType = accountingAccount.AccountingAccountType,
                                DetailedType = accountingAccount.DetailedType,
                                Balance = accountingAccount.Balance,
                                AcceptData = accountingAccount.AcceptData,
                                UseCostCenter = accountingAccount.UseCostCenter,
                                UseThird = accountingAccount.UseThird,
                                DateLastUpdate = DateTime.UtcNow,
                                DateHour = DateTime.UtcNow,
                                AccountingAccountCode = accountingAccount.AccountingAccountCode,
                                CompanyId = accountingAccount.CompanyId,
                                DescriptionIFRS = accountingAccount.DescriptionIFRS
                            };
                            await _db.ACCOUNTING_ACCOUNT.AddAsync(accountingAccountToCreate);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- DEFAULT COSTS CENTERS AND ACCOUNTING ACCOUNTS RELATIONSHIP --------------------------------

                    var costCenterPeopleAndCultureOce = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "10-02-04" && x.CompanyId == "OCE");
                    var costCenterPeopleAndCultureLLC = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "10-02-04" && x.CompanyId == "LLC");
                    var costCenterFinancesOce = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "50-01-00" && x.CompanyId == "OCE");
                    var costCenterFinancesLLC = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "50-01-00" && x.CompanyId == "LLC");
                    var costCenterRecruitingOce = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "30-01-01" && x.CompanyId == "OCE");
                    var costCenterRecruitingLLC = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "30-01-01" && x.CompanyId == "LLC");
                    var costCenterEjecutivoCuentasOce = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "10-02-02" && x.CompanyId == "OCE");
                    var costCenterEjecutivoCuentasLLC = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "10-02-02" && x.CompanyId == "LLC");
                    var costCenterOperacionesRecursosOce = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "40-01-01" && x.CompanyId == "OCE");
                    var costCenterOperacionesRecursosLLC = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "40-01-01" && x.CompanyId == "LLC");

                    var accountingAccountReservaBonuslyOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "3-02-02-000-000" && x.CompanyId == "OCE");
                    var accountingAccountBalanceProgramOce = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "3-02-01-000-000" && x.CompanyId == "OCE");
                    var accountingAccountOceansChallengeOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "6-01-03-005-000" && x.CompanyId == "OCE");
                    var accountingAccountOceansChallengeLLC = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "6-01-04-013-0000" && x.CompanyId == "LLC");
                    var accountingAccountPagosAdministrativosOce = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "6-01-01-001-000" && x.CompanyId == "OCE");
                    var accountingAccountPagosAdministrativosLLC = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "6-01-04-013-0000" && x.CompanyId == "LLC");
                    var accountingAccountFeriadosAdministrativosOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "6-01-01-002-000" && x.CompanyId == "OCE");
                    var accountingAccountInterviewHoursOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "6-01-01-005-000" && x.CompanyId == "OCE");
                    var accountingAccountPagoEjecutivoDeCuentasOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "6-01-01-007-000" && x.CompanyId == "OCE");
                    var accountingAccountConsultantHoursOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "5-01-01-000-000" && x.CompanyId == "OCE");
                    var accountingAccountConsultantHoursLLC = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "5-01-01-000-0000" && x.CompanyId == "LLC");
                    var accountingAccountConsultantHolidaysOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "5-01-06-000-000" && x.CompanyId == "OCE");
                    var accountingAccountConsultantHolidaysLLC = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "5-01-03-000-0000" && x.CompanyId == "LLC");
                    var accountingAccountOnCallFlateRateOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "5-01-16-000-000" && x.CompanyId == "OCE");
                    var accountingAccountOnCallFlateRateLLC = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "5-01-08-000-0000" && x.CompanyId == "LLC");
                    var accountingAccountOnCallTimeWorkedOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "5-01-22-000-000" && x.CompanyId == "OCE");
                    var accountingAccountOnCallTimeWorkedLLC = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "5-01-09-000-0000" && x.CompanyId == "LLC");

                    List<CostCenterAccountingAccount> costsCentersaccountingAccountsList = new List<CostCenterAccountingAccount>
                {
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterPeopleAndCultureOce.CostCenterId,
    AccountingAccountId = accountingAccountBalanceProgramOce.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterPeopleAndCultureLLC.CostCenterId,
    AccountingAccountId = accountingAccountOceansChallengeLLC.AccountingAccountId,
    Status = "A",
    CompanyId = "LLC"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterPeopleAndCultureOce.CostCenterId,
    AccountingAccountId = accountingAccountOceansChallengeOCE.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterPeopleAndCultureOce.CostCenterId,
    AccountingAccountId = accountingAccountReservaBonuslyOCE.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterFinancesOce.CostCenterId,
    AccountingAccountId = accountingAccountPagosAdministrativosOce.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterFinancesLLC.CostCenterId,
    AccountingAccountId = accountingAccountPagosAdministrativosLLC.AccountingAccountId,
    Status = "A",
    CompanyId = "LLC"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterFinancesOce.CostCenterId,
    AccountingAccountId = accountingAccountFeriadosAdministrativosOCE.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterRecruitingOce.CostCenterId,
    AccountingAccountId = accountingAccountPagosAdministrativosOce.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterRecruitingLLC.CostCenterId,
    AccountingAccountId = accountingAccountPagosAdministrativosLLC.AccountingAccountId,
    Status = "A",
    CompanyId = "LLC"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterPeopleAndCultureOce.CostCenterId,
    AccountingAccountId = accountingAccountFeriadosAdministrativosOCE.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterRecruitingOce.CostCenterId,
    AccountingAccountId = accountingAccountInterviewHoursOCE.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterRecruitingOce.CostCenterId,
    AccountingAccountId = accountingAccountFeriadosAdministrativosOCE.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterEjecutivoCuentasOce.CostCenterId,
    AccountingAccountId = accountingAccountPagoEjecutivoDeCuentasOCE.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterEjecutivoCuentasOce.CostCenterId,
    AccountingAccountId = accountingAccountFeriadosAdministrativosOCE.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterEjecutivoCuentasLLC.CostCenterId,
    AccountingAccountId = accountingAccountPagosAdministrativosLLC.AccountingAccountId,
    Status = "A",
    CompanyId = "LLC"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterOperacionesRecursosOce.CostCenterId,
    AccountingAccountId = accountingAccountConsultantHoursOCE.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterOperacionesRecursosLLC.CostCenterId,
    AccountingAccountId = accountingAccountConsultantHoursLLC.AccountingAccountId,
    Status = "A",
    CompanyId = "LLC"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterOperacionesRecursosOce.CostCenterId,
    AccountingAccountId = accountingAccountConsultantHolidaysOCE.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterOperacionesRecursosLLC.CostCenterId,
    AccountingAccountId = accountingAccountConsultantHolidaysLLC.AccountingAccountId,
    Status = "A",
    CompanyId = "LLC"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterOperacionesRecursosOce.CostCenterId,
    AccountingAccountId = accountingAccountOnCallFlateRateOCE.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterOperacionesRecursosLLC.CostCenterId,
    AccountingAccountId = accountingAccountOnCallFlateRateLLC.AccountingAccountId,
    Status = "A",
    CompanyId = "LLC"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterOperacionesRecursosOce.CostCenterId,
    AccountingAccountId = accountingAccountOnCallTimeWorkedOCE.AccountingAccountId,
    Status = "A",
    CompanyId = "OCE"
    },
                    new CostCenterAccountingAccount {
    CostCenterId = costCenterOperacionesRecursosLLC.CostCenterId,
    AccountingAccountId = accountingAccountOnCallTimeWorkedLLC.AccountingAccountId,
    Status = "A",
    CompanyId = "LLC"
    }
                };

                    foreach (var ccaa in costsCentersaccountingAccountsList)
                    {
                        var existingRelationship = await _db.COSTS_CENTERS_ACCOUNTING_ACCOUNTS.FirstOrDefaultAsync(x => x.CostCenterId == ccaa.CostCenterId
                        && x.AccountingAccountId == ccaa.AccountingAccountId && x.CompanyId == ccaa.CompanyId);
                        if (existingRelationship == null)
                        {
                            CostCenterAccountingAccount ccAaToCreate = new()
                            {
                                CostCenterId = ccaa.CostCenterId,
                                AccountingAccountId = ccaa.AccountingAccountId,
                                Status = ccaa.Status,
                                CreateDate = DateTime.UtcNow,
                                CompanyId = ccaa.CompanyId
                            };
                            await _db.COSTS_CENTERS_ACCOUNTING_ACCOUNTS.AddAsync(ccAaToCreate);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- CONSULTANT BENEFITS COMPANIES --------------------------------

                    List<ConsultantBenefitCompany> consultantBenefitCompaniesList = new List<ConsultantBenefitCompany>();
                    var peopleAndCultureCostCenterOCE = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "10-02-04" && x.CompanyId == "OCE");
                    var peopleAndCultureCostCenterLLC = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "10-02-04" && x.CompanyId == "LLC");
                    var accountingAccountReservaBalanceProgramOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "3-02-01-000-000" && x.CompanyId == "OCE");
                    var accountingAccountAdminExpensesLLC = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "6-01-04-013-0000" && x.CompanyId == "LLC");

                    // OCE
                    consultantBenefitCompaniesList.Add(new ConsultantBenefitCompany
                    {
                        CompanyId = "OCE",
                        CostCenterId = peopleAndCultureCostCenterOCE.CostCenterId,
                        AccountingAccountId = accountingAccountReservaBalanceProgramOCE.AccountingAccountId,
                        BenefitId = balanceProgramBenefit.BenefitId
                    });
                    consultantBenefitCompaniesList.Add(new ConsultantBenefitCompany
                    {
                        CompanyId = "OCE",
                        CostCenterId = peopleAndCultureCostCenterOCE.CostCenterId,
                        AccountingAccountId = accountingAccountReservaBonuslyOCE.AccountingAccountId,
                        BenefitId = bonuslyBenefit.BenefitId
                    });
                    consultantBenefitCompaniesList.Add(new ConsultantBenefitCompany
                    {
                        CompanyId = "OCE",
                        CostCenterId = peopleAndCultureCostCenterOCE.CostCenterId,
                        AccountingAccountId = accountingAccountOceansChallengeOCE.AccountingAccountId,
                        BenefitId = oceansChallengeBenefit.BenefitId
                    });
                    // LLC
                    consultantBenefitCompaniesList.Add(new ConsultantBenefitCompany
                    {
                        CompanyId = "LLC",
                        CostCenterId = peopleAndCultureCostCenterLLC.CostCenterId,
                        AccountingAccountId = accountingAccountAdminExpensesLLC.AccountingAccountId,
                        BenefitId = balanceProgramBenefit.BenefitId
                    });
                    consultantBenefitCompaniesList.Add(new ConsultantBenefitCompany
                    {
                        CompanyId = "LLC",
                        CostCenterId = peopleAndCultureCostCenterLLC.CostCenterId,
                        AccountingAccountId = accountingAccountAdminExpensesLLC.AccountingAccountId,
                        BenefitId = bonuslyBenefit.BenefitId
                    });
                    consultantBenefitCompaniesList.Add(new ConsultantBenefitCompany
                    {
                        CompanyId = "LLC",
                        CostCenterId = peopleAndCultureCostCenterLLC.CostCenterId,
                        AccountingAccountId = accountingAccountAdminExpensesLLC.AccountingAccountId,
                        BenefitId = oceansChallengeBenefit.BenefitId
                    });

                    foreach (var benefitCompany in consultantBenefitCompaniesList)
                    {
                        var existingBenefitCompany = await _db.CONSULTANT_BENEFIT_COMPANIES.FirstOrDefaultAsync(x => x.CompanyId == benefitCompany.CompanyId &&
                        x.CostCenterId == benefitCompany.CostCenterId && x.AccountingAccountId == benefitCompany.AccountingAccountId && x.BenefitId == benefitCompany.BenefitId);
                        if (existingBenefitCompany == null)
                        {
                            ConsultantBenefitCompany conBenefitCompany = new()
                            {
                                CompanyId = benefitCompany.CompanyId,
                                CostCenterId = benefitCompany.CostCenterId,
                                AccountingAccountId = benefitCompany.AccountingAccountId,
                                BenefitId = benefitCompany.BenefitId
                            };
                            await _db.CONSULTANT_BENEFIT_COMPANIES.AddAsync(conBenefitCompany);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- REPORTING MY TIME MOVEMENT TYPES --------------------------------

                    List<ReportingMyTimeMovementType> movTypesList = new List<ReportingMyTimeMovementType>
                {
                    new ReportingMyTimeMovementType { Name = "Normal Hours", IsPayable = true },
                    new ReportingMyTimeMovementType { Name = "On Call Flate Rate", IsPayable = true },
                    new ReportingMyTimeMovementType { Name = "On Call Time Worked", IsPayable = true },
                    new ReportingMyTimeMovementType { Name = "Balance Program", IsPayable = true },
                    new ReportingMyTimeMovementType { Name = "Oceans Challenge", IsPayable = true },
                    new ReportingMyTimeMovementType { Name = "Bonusly Rewards", IsPayable = true },
                    new ReportingMyTimeMovementType { Name = "Interviews", IsPayable = true },
                    new ReportingMyTimeMovementType { Name = "Time Off (Non-payable)", IsPayable = false },
                    new ReportingMyTimeMovementType { Name = "Holidays", IsPayable = true },
                    new ReportingMyTimeMovementType { Name = "Overtime Hours", IsPayable = false }
                };

                    foreach (var movementType in movTypesList)
                    {
                        var existingType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.Name == movementType.Name);
                        if (existingType == null)
                        {
                            ReportingMyTimeMovementType movType = new()
                            {
                                Name = movementType.Name,
                                IsPayable = movementType.IsPayable
                            };
                            await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.AddAsync(movType);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- CONSULTANT POSITIONS BY DEFAULT --------------------------------

                    var existingConsultantPositionsList = await _db.CONSULTANT_POSITIONS.AnyAsync();
                    if (!existingConsultantPositionsList)
                    {
                        List<CreatePositionsWithAccountingConfigVM> consultantPositionsList = new List<CreatePositionsWithAccountingConfigVM>();
                        //People and culture coordinator
                        consultantPositionsList.Add(new CreatePositionsWithAccountingConfigVM
                        {
                            Name = "People and Culture Coordinator",
                            IsAdministrative = true,
                            AccountingConfig = new List<CreateAccountingConfigVM>
                        {   //LLC
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Balance Program",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Bonusly Rewards",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Holidays",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Interviews",
                                CompanyId = "LLC",
                                CostCenterCode = "30-01-01",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Normal Hours",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Oceans Challenge",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Flate Rate",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Time Worked",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },//OCE
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Balance Program",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "3-02-01-000-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Bonusly Rewards",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "3-02-02-000-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Holidays",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-01-002-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Interviews",
                                CompanyId = "OCE",
                                CostCenterCode = "30-01-01",
                                AccountingAccountCode = "6-01-01-005-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Normal Hours",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-01-001-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Oceans Challenge",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-03-005-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Flate Rate",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-01-001-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Time Worked",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-01-001-000"
                            }
                        }
                        });
                        //Success Manager
                        consultantPositionsList.Add(new CreatePositionsWithAccountingConfigVM
                        {
                            Name = "Success Manager",
                            IsAdministrative = true,
                            AccountingConfig = new List<CreateAccountingConfigVM>
                        {   //LLC
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Balance Program",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Bonusly Rewards",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Holidays",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-02",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Interviews",
                                CompanyId = "LLC",
                                CostCenterCode = "30-01-01",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Normal Hours",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-02",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Oceans Challenge",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Flate Rate",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-02",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Time Worked",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-02",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },//OCE
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Balance Program",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "3-02-01-000-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Bonusly Rewards",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "3-02-02-000-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Holidays",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-02",
                                AccountingAccountCode = "6-01-01-002-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Interviews",
                                CompanyId = "OCE",
                                CostCenterCode = "30-01-01",
                                AccountingAccountCode = "6-01-01-005-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Normal Hours",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-02",
                                AccountingAccountCode = "6-01-01-007-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Oceans Challenge",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-03-005-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Flate Rate",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-02",
                                AccountingAccountCode = "6-01-01-007-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Time Worked",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-02",
                                AccountingAccountCode = "6-01-01-007-000"
                            }
                        }
                        });
                        //Finance Assistant
                        consultantPositionsList.Add(new CreatePositionsWithAccountingConfigVM
                        {
                            Name = "Finance Assistant",
                            IsAdministrative = true,
                            AccountingConfig = new List<CreateAccountingConfigVM>
                        {   //LLC
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Balance Program",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Bonusly Rewards",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Holidays",
                                CompanyId = "LLC",
                                CostCenterCode = "50-01-00",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Interviews",
                                CompanyId = "LLC",
                                CostCenterCode = "30-01-01",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Normal Hours",
                                CompanyId = "LLC",
                                CostCenterCode = "50-01-00",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Oceans Challenge",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Flate Rate",
                                CompanyId = "LLC",
                                CostCenterCode = "50-01-00",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Time Worked",
                                CompanyId = "LLC",
                                CostCenterCode = "50-01-00",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },//OCE
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Balance Program",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "3-02-01-000-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Bonusly Rewards",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "3-02-02-000-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Holidays",
                                CompanyId = "OCE",
                                CostCenterCode = "50-01-00",
                                AccountingAccountCode = "6-01-01-002-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Interviews",
                                CompanyId = "OCE",
                                CostCenterCode = "30-01-01",
                                AccountingAccountCode = "6-01-01-005-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Normal Hours",
                                CompanyId = "OCE",
                                CostCenterCode = "50-01-00",
                                AccountingAccountCode = "6-01-01-001-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Oceans Challenge",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-03-005-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Flate Rate",
                                CompanyId = "OCE",
                                CostCenterCode = "50-01-00",
                                AccountingAccountCode = "6-01-01-001-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Time Worked",
                                CompanyId = "OCE",
                                CostCenterCode = "50-01-00",
                                AccountingAccountCode = "6-01-01-001-000"
                            }
                        }
                        });
                        //Full Stack Developer
                        consultantPositionsList.Add(new CreatePositionsWithAccountingConfigVM
                        {
                            Name = "Full Stack Developer",
                            IsAdministrative = false,
                            AccountingConfig = new List<CreateAccountingConfigVM>
                        {   //LLC
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Balance Program",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Bonusly Rewards",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Holidays",
                                CompanyId = "LLC",
                                CostCenterCode = "40-01-01",
                                AccountingAccountCode = "5-01-03-000-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Interviews",
                                CompanyId = "LLC",
                                CostCenterCode = "30-01-01",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Normal Hours",
                                CompanyId = "LLC",
                                CostCenterCode = "40-01-01",
                                AccountingAccountCode = "5-01-01-000-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Oceans Challenge",
                                CompanyId = "LLC",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-04-013-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Flate Rate",
                                CompanyId = "LLC",
                                CostCenterCode = "40-01-01",
                                AccountingAccountCode = "5-01-08-000-0000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Time Worked",
                                CompanyId = "LLC",
                                CostCenterCode = "40-01-01",
                                AccountingAccountCode = "5-01-09-000-0000"
                            },//OCE
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Balance Program",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "3-02-01-000-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Bonusly Rewards",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "3-02-02-000-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Holidays",
                                CompanyId = "OCE",
                                CostCenterCode = "40-01-01",
                                AccountingAccountCode = "5-01-06-000-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Interviews",
                                CompanyId = "OCE",
                                CostCenterCode = "30-01-01",
                                AccountingAccountCode = "6-01-01-005-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Normal Hours",
                                CompanyId = "OCE",
                                CostCenterCode = "40-01-01",
                                AccountingAccountCode = "5-01-01-000-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "Oceans Challenge",
                                CompanyId = "OCE",
                                CostCenterCode = "10-02-04",
                                AccountingAccountCode = "6-01-03-005-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Flate Rate",
                                CompanyId = "OCE",
                                CostCenterCode = "40-01-01",
                                AccountingAccountCode = "5-01-16-000-000"
                            },
                            new CreateAccountingConfigVM
                            {
                                MovementTypeName = "On Call Time Worked",
                                CompanyId = "OCE",
                                CostCenterCode = "40-01-01",
                                AccountingAccountCode = "5-01-22-000-000"
                            }
                        }
                        });

                        foreach (var position in consultantPositionsList)
                        {
                            ConsultantPosition positionToCreate = new()
                            {
                                Name = position.Name,
                                IsAdministrative = position.IsAdministrative
                            };
                            var createdPositon = await _db.CONSULTANT_POSITIONS.AddAsync(positionToCreate);
                            await _db.SaveChangesAsync();
                            if (createdPositon.Entity.ConsultantPositionId > 0)
                            {
                                foreach (var aConfig in position.AccountingConfig)
                                {
                                    var costCenter = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == aConfig.CostCenterCode && x.CompanyId == aConfig.CompanyId);
                                    var accountingAccount = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == aConfig.AccountingAccountCode && x.CompanyId == aConfig.CompanyId);
                                    var movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.Name == aConfig.MovementTypeName);

                                    ConsultantPositionAccountingConfiguration accountingConfigToCreate = new()
                                    {
                                        CompanyId = aConfig.CompanyId,
                                        CostCenterId = costCenter.CostCenterId,
                                        AccountingAccountId = accountingAccount.AccountingAccountId,
                                        MovementTypeId = movementType.MovementTypeId,
                                        PositionId = createdPositon.Entity.ConsultantPositionId
                                    };
                                    await _db.CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION.AddAsync(accountingConfigToCreate);
                                }
                                await _db.SaveChangesAsync();
                            }
                        }
                    }

                    // ----------------- TRANSACTION TYPES --------------------------------

                    List<TransactionType> transactionTypesList = new List<TransactionType>
                {
                    new TransactionType { Name = "Debit" },
                    new TransactionType { Name = "Credit" }
                };

                    foreach (var type in transactionTypesList)
                    {
                        var existingType = await _db.TRANSACTION_TYPES.FirstOrDefaultAsync(x => x.Name == type.Name);
                        if (existingType == null)
                        {
                            TransactionType transactionType = new()
                            {
                                Name = type.Name
                            };
                            await _db.TRANSACTION_TYPES.AddAsync(transactionType);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- DOCUMENT TYPES --------------------------------

                    List<DocumentType> documentTypesList = new List<DocumentType>
                {
                    new DocumentType {DocumentTypeId = "FAC", TransactionTypeId = 1, Description = "Invoice" },
                    new DocumentType {DocumentTypeId = "I/C", TransactionTypeId = 1, Description = "Current Interest" },
                    new DocumentType {DocumentTypeId = "INT", TransactionTypeId = 1, Description = "Late Payment Interest" },
                    new DocumentType {DocumentTypeId = "L/C", TransactionTypeId = 1, Description = "Bill of Exchange" },
                    new DocumentType {DocumentTypeId = "N/D", TransactionTypeId = 1, Description = "Debit Note" },
                    new DocumentType {DocumentTypeId = "O/D", TransactionTypeId = 1, Description = "Other Debit" },
                    new DocumentType {DocumentTypeId = "PAG", TransactionTypeId = 1, Description = "Promissory Note" },
                    new DocumentType {DocumentTypeId = "DEP", TransactionTypeId = 2, Description = "Deposit" },
                    new DocumentType {DocumentTypeId = "N/C", TransactionTypeId = 2, Description = "Credit Note" },
                    new DocumentType {DocumentTypeId = "O/C", TransactionTypeId = 2, Description = "Other Credit" },
                    new DocumentType {DocumentTypeId = "TEF", TransactionTypeId = 2, Description = "Transfer" }
                };
                    foreach (var docType in documentTypesList)
                    {
                        if (await _db.DOCUMENTS_TYPES.FirstOrDefaultAsync(x => x.Description == docType.Description && x.TransactionTypeId == docType.TransactionTypeId &&
                        x.DocumentTypeId == docType.DocumentTypeId) == null)
                        {
                            DocumentType documentType = new()
                            {
                                DocumentTypeId = docType.DocumentTypeId,
                                Description = docType.Description,
                                TransactionTypeId = docType.TransactionTypeId
                            };
                            await _db.DOCUMENTS_TYPES.AddAsync(documentType);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- GLOBAL CONSECUTIVES --------------------------------

                    List<GlobalConsecutive> globalConsecutivesList = new List<GlobalConsecutive>
                {
                    new GlobalConsecutive { Name = "JOURNAL_CXP", ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new GlobalConsecutive { Name = "JOURNAL_CXP", ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new GlobalConsecutive { Name = "FAC", ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new GlobalConsecutive { Name = "FAC", ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new GlobalConsecutive { Name = "I/C", ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new GlobalConsecutive { Name = "I/C", ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new GlobalConsecutive { Name = "INT", ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new GlobalConsecutive { Name = "INT", ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new GlobalConsecutive { Name = "L/C", ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new GlobalConsecutive { Name = "L/C", ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new GlobalConsecutive { Name = "N/D", ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new GlobalConsecutive { Name = "N/D", ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new GlobalConsecutive { Name = "O/D", ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new GlobalConsecutive { Name = "O/D", ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new GlobalConsecutive { Name = "PAG", ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new GlobalConsecutive { Name = "PAG", ConsecutiveNumber = 0, CompanyId = "LLC" },
                    new GlobalConsecutive { Name = "PRODUCTS", ConsecutiveNumber = 3, CompanyId = "OCE" }
                };

                    foreach (var consecutive in globalConsecutivesList)
                    {
                        var existingConsecutive = await _db.GLOBAL_CONSECUTIVES.FirstOrDefaultAsync(x => x.Name == consecutive.Name);
                        if (existingConsecutive == null)
                        {
                            GlobalConsecutive conToCreate = new()
                            {
                                Name = consecutive.Name,
                                ConsecutiveNumber = consecutive.ConsecutiveNumber,
                                CompanyId = consecutive.CompanyId
                            };
                            await _db.GLOBAL_CONSECUTIVES.AddAsync(conToCreate);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- DEFAULT CLIENT FOR TESTING --------------------------------
                    var existingClientsList = await _db.CLIENT.AnyAsync();
                    if (!existingClientsList)
                    {
                        Client client = new()
                        {
                            Name = "Client for testing",
                            ClientCode = "OCE_001",
                            Alias = "Client Test",
                            AdmissionDate = DateTime.Now,
                            PaymentCondition = "7",
                            Discount = 0,
                            IsActive = "S",
                            ClientCategory = "EXT",
                            CreationDate = DateTime.Now,
                            CompanyId = "OCE",
                            LatePaymentFee = 0,
                            AllowSentLatePaymentNotifications = true
                        };
                        await _db.CLIENT.AddAsync(client);
                        await _db.SaveChangesAsync();
                    }
                    // ----------------- DEFAULT CLIENT FOR ADMINISTRATIVE CONSULTANTS --------------------------------

                    if (await _db.CLIENT.FirstOrDefaultAsync(x => x.Name == "Oceans Code Experts") == null)
                    {
                        Client client = new()
                        {
                            Name = "Oceans Code Experts",
                            ClientCode = "OCEADMIN01",
                            Alias = "Oceans Code Experts",
                            AdmissionDate = DateTime.Now,
                            PaymentCondition = "ND",
                            Discount = 0,
                            IsActive = "S",
                            ClientCategory = "OCEADMIN",
                            CreationDate = DateTime.Now,
                            CompanyId = "OCE/LLC",
                            LatePaymentFee = 0,
                            AllowSentLatePaymentNotifications = false
                        };
                        await _db.CLIENT.AddAsync(client);
                        await _db.SaveChangesAsync();
                    }

                    // ----------------- PARTNERS --------------------------------

                    List<Partner> partnersList = new List<Partner>
                {
                    new Partner
                    {
                        Name = "Global Business",
                        Contact = "Yeanett Russo",
                        ContactOccupation = "Admin and Finances",
                        ContactEmail = "yeanett.russo@gbitcorp.com",
                        Phone = "(+507) 310 2673",
                        AdmissionDate = DateTime.Parse("2022-02-07"),
                        IsActive = true,
                        CreationDate = DateTime.UtcNow,
                        CompanyId = "OCE",
                        IdCountry = "PAN"
                    },
                    new Partner
                    {
                        Name = "Syntepro",
                        Contact = "Andrés Ureña",
                        ContactOccupation = "Accountant",
                        ContactEmail = "andres.urena@gruposyntepro.com",
                        Phone = "(+506) 2101 8823",
                        AdmissionDate = DateTime.Parse("2021-06-01"),
                        IsActive = true,
                        CreationDate = DateTime.UtcNow,
                        CompanyId = "OCE",
                        IdCountry = "CRI"
                    }
                };

                    foreach (var partner in partnersList)
                    {
                        var existingNS = await _db.PARTNERS.FirstOrDefaultAsync(x => x.Name == partner.Name);
                        if (existingNS == null)
                        {
                            Partner partnerToCreate = partner;
                            await _db.PARTNERS.AddAsync(partnerToCreate);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- PAYMENT METHODS --------------------------------

                    List<PaymentMethod> paymentMethodsList = new List<PaymentMethod>
                {
                    new PaymentMethod { Name = "Bac Credomatic different from Panamá (Ameritransfer)", CompanyId = "OCE" },
                    new PaymentMethod { Name = "Other banks (International Transfer)", CompanyId = "OCE" },
                    new PaymentMethod { Name = "Payoneer", CompanyId = "OCE" },
                    new PaymentMethod { Name = "Banco General (Panamá)", CompanyId = "OCE" },
                    new PaymentMethod { Name = "Bac Credomatic (Panamá)", CompanyId = "OCE" },
                    new PaymentMethod { Name = "Mercury", CompanyId = "LLC" },
                    new PaymentMethod { Name = "Wise", CompanyId = "LLC" },
                    new PaymentMethod { Name = "Bac Credomatic Costa Rica (Bac CR to Bac CR)", CompanyId = "OCE" },
                    new PaymentMethod { Name = "USA local transfer", CompanyId = "LLC" }
                };

                    foreach (var paymentMethod in paymentMethodsList)
                    {
                        if (await _db.PAYMENT_METHODS.FirstOrDefaultAsync(x => x.Name == paymentMethod.Name) == null)
                        {
                            PaymentMethod pm = new()
                            {
                                Name = paymentMethod.Name,
                                CompanyId = paymentMethod.CompanyId
                            };
                            await _db.PAYMENT_METHODS.AddAsync(pm);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- DEFAULT BANK ACCOUNTS --------------------------------

                    List<BankAccount> bankAccountsList = new List<BankAccount>
                {
                    new BankAccount { BankAccountCode = "113439285", BankAccountName = "Cta # 113439285  BAC CREDOMATIC PANAMA", IsActive = "S", CompanyId = "OCE" },
                    new BankAccount { BankAccountCode = "202218366303", BankAccountName = "Cta #202218366303 Mercury", IsActive = "S", CompanyId = "LLC" },
                };

                    foreach (var bankAccount in bankAccountsList)
                    {
                        if (await _db.BANK_ACCOUNTS.FirstOrDefaultAsync(x => x.BankAccountCode == bankAccount.BankAccountCode) == null)
                        {
                            BankAccount ba = new()
                            {
                                BankAccountCode = bankAccount.BankAccountCode,
                                BankAccountName = bankAccount.BankAccountName,
                                IsActive = bankAccount.IsActive,
                                CompanyId = bankAccount.CompanyId
                            };
                            await _db.BANK_ACCOUNTS.AddAsync(ba);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- PAYMENT METHOD BANK ACCOUNTS --------------------------------

                    var paymentMethods = await _db.PAYMENT_METHODS.ToListAsync();
                    var bankAccounts = await _db.BANK_ACCOUNTS.ToListAsync();

                    List<PaymentMethodBankAccount> paymentMethodBankAccountList = new List<PaymentMethodBankAccount>
                {
                    new PaymentMethodBankAccount {
                        PaymentMethodId = paymentMethods.FirstOrDefault(x => x.Name == "Bac Credomatic different from Panamá (Ameritransfer)").PaymentMethodId,
                        BankAccountId = bankAccounts.FirstOrDefault(x => x.BankAccountCode == "113439285").BankAccountId,
                        IsDefault = true
                    },
                    new PaymentMethodBankAccount {
                        PaymentMethodId = paymentMethods.FirstOrDefault(x => x.Name == "Other banks (International Transfer)").PaymentMethodId,
                        BankAccountId = bankAccounts.FirstOrDefault(x => x.BankAccountCode == "113439285").BankAccountId,
                        IsDefault = true
                    },
                    new PaymentMethodBankAccount {
                        PaymentMethodId = paymentMethods.FirstOrDefault(x => x.Name == "Banco General (Panamá)").PaymentMethodId,
                        BankAccountId = bankAccounts.FirstOrDefault(x => x.BankAccountCode == "113439285").BankAccountId,
                        IsDefault = true
                    },
                    new PaymentMethodBankAccount {
                        PaymentMethodId = paymentMethods.FirstOrDefault(x => x.Name == "Bac Credomatic (Panamá)").PaymentMethodId,
                        BankAccountId = bankAccounts.FirstOrDefault(x => x.BankAccountCode == "113439285").BankAccountId,
                        IsDefault = true
                    },
                    new PaymentMethodBankAccount {
                        PaymentMethodId = paymentMethods.FirstOrDefault(x => x.Name == "Mercury").PaymentMethodId,
                        BankAccountId = bankAccounts.FirstOrDefault(x => x.BankAccountCode == "202218366303").BankAccountId,
                        IsDefault = true
                    }
                };

                    foreach (var paymentMethodBankAccount in paymentMethodBankAccountList)
                    {
                        if (await _db.PAYMENT_METHOD_AND_BANK_ACCOUNTS.FirstOrDefaultAsync(x => x.PaymentMethodId == paymentMethodBankAccount.PaymentMethodId &&
                        x.BankAccountId == paymentMethodBankAccount.BankAccountId) == null)
                        {
                            PaymentMethodBankAccount pmba = new()
                            {
                                PaymentMethodId = paymentMethodBankAccount.PaymentMethodId,
                                BankAccountId = paymentMethodBankAccount.BankAccountId,
                                IsDefault = paymentMethodBankAccount.IsDefault
                            };
                            await _db.PAYMENT_METHOD_AND_BANK_ACCOUNTS.AddAsync(pmba);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- PRODUCTS BY DEFAULT --------------------------------

                    List<Product> productsList = new List<Product>
                {
                    new Product { Name = "Hours of professional services", ProductCode = "PR_0000001", Alias = "Hours of professional services" },
                    new Product { Name = "On Call Flate Rate", ProductCode = "PR_0000002", Alias = "On Call Flate Rate" },
                    new Product { Name = "On Call Time Worked", ProductCode = "PR_0000003", Alias = "On Call Time Worked" },
                    new Product { Name = "Hours of professional services(Overtime)", ProductCode = "PR_0000004", Alias = "Hours of professional services(Overtime)" }
                };

                    foreach (var product in productsList)
                    {
                        if (await _db.PRODUCTS.FirstOrDefaultAsync(x => x.Name == product.Name) == null)
                        {
                            Product pm = new()
                            {
                                Name = product.Name,
                                ProductCode = product.ProductCode,
                                Alias = product.Alias
                            };
                            await _db.PRODUCTS.AddAsync(pm);
                        }
                    }
                    await _db.SaveChangesAsync();


                    // ----------------- TRANSACTION STATUSES --------------------------------

                    List<TransactionStatus> transactionStatusesList = new List<TransactionStatus>
                {
                    new TransactionStatus { Name = "No actions" },
                    new TransactionStatus { Name = "Waiting to be approved" },
                    new TransactionStatus { Name = "Approved" },
                    new TransactionStatus { Name = "Rejected" },
                    new TransactionStatus { Name = "Sent to be paid" },
                    new TransactionStatus { Name = "Paid" },
                    new TransactionStatus { Name = "Pending Accounting" },
                    new TransactionStatus { Name = "Accounted" },
                    new TransactionStatus { Name = "Updated - Pending Review" }
                };

                    foreach (var status in transactionStatusesList)
                    {
                        var existingStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == status.Name);
                        if (existingStatus == null)
                        {
                            TransactionStatus transactionStatus = new()
                            {
                                Name = status.Name
                            };
                            await _db.TRANSACTION_STATUSES.AddAsync(transactionStatus);
                        }
                    }
                    await _db.SaveChangesAsync();

                    //---------------SYSTEM ARCHITECTURE------------------------------
                    // ----------------- SYSTEM AREAS --------------------------------

                    List<SystemArea> systemAreasList = new List<SystemArea>
                {
                    new SystemArea { Name = "Admin Center" },
                    new SystemArea { Name = "Finances" },
                    new SystemArea { Name = "General" },
                    new SystemArea { Name = "Tracking Tool" },
                    new SystemArea { Name = "Dashboard" },
                    new SystemArea { Name = "My Account" },
                    new SystemArea { Name = "Account Management" },
                    new SystemArea { Name = "Recruiting" }
                };

                    foreach (var area in systemAreasList)
                    {
                        if (await _db.SYSTEM_AREAS.FirstOrDefaultAsync(x => x.Name == area.Name) == null)
                        {
                            SystemArea sa = new()
                            {
                                Name = area.Name
                            };
                            await _db.SYSTEM_AREAS.AddAsync(sa);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- SYSTEM SUB AREAS --------------------------------

                    var existingSystemAreasList = await _db.SYSTEM_AREAS.ToListAsync();

                    var adminCenterAreaId = existingSystemAreasList.FirstOrDefault(x => x.Name == "Admin Center").SystemAreaId;
                    var financesAreaId = existingSystemAreasList.FirstOrDefault(x => x.Name == "Finances").SystemAreaId;
                    var generalAreaId = existingSystemAreasList.FirstOrDefault(x => x.Name == "General").SystemAreaId;
                    var trackingToolAreaId = existingSystemAreasList.FirstOrDefault(x => x.Name == "Tracking Tool").SystemAreaId;
                    var dashboardAreaId = existingSystemAreasList.FirstOrDefault(x => x.Name == "Dashboard").SystemAreaId;
                    var myAccountAreaId = existingSystemAreasList.FirstOrDefault(x => x.Name == "My Account").SystemAreaId;
                    var accountManagementAreaId = existingSystemAreasList.FirstOrDefault(x => x.Name == "Account Management").SystemAreaId;
                    var recruitingAreaId = existingSystemAreasList.FirstOrDefault(x => x.Name == "Recruiting").SystemAreaId;

                    List<SystemSubArea> systemSubAreasList = new List<SystemSubArea>
                {
                    new SystemSubArea { SystemAreaId = adminCenterAreaId, Name = "Actualizar Datos desde Softland" },
                    new SystemSubArea { SystemAreaId = adminCenterAreaId, Name = "Roles y Permisos de Usuarios" },
                    new SystemSubArea { SystemAreaId = adminCenterAreaId, Name = "Consultant Positions Accounting Configuration" },
                    new SystemSubArea { SystemAreaId = financesAreaId, Name = "Cuentas Por Cobrar" },
                    new SystemSubArea { SystemAreaId = financesAreaId, Name = "Consultant Payment Debits & Credits" },
                    new SystemSubArea { SystemAreaId = financesAreaId, Name = "Payment Sheets" },
                    new SystemSubArea { SystemAreaId = financesAreaId, Name = "Export Accounting Data" },
                    new SystemSubArea { SystemAreaId = financesAreaId, Name = "Calculadora Financiera" },
                    new SystemSubArea { SystemAreaId = generalAreaId, Name = "Consultants" },
                    new SystemSubArea { SystemAreaId = generalAreaId, Name = "Consultant Reimbursed Benefits" },
                    new SystemSubArea { SystemAreaId = generalAreaId, Name = "Holidays" },
                    new SystemSubArea { SystemAreaId = trackingToolAreaId, Name = "Reporting My Time" },
                    new SystemSubArea { SystemAreaId = trackingToolAreaId, Name = "General Reports" },
                    new SystemSubArea { SystemAreaId = trackingToolAreaId, Name = "My Reports History" },
                    new SystemSubArea { SystemAreaId = dashboardAreaId, Name = "Dashboard" },
                    new SystemSubArea { SystemAreaId = myAccountAreaId, Name = "Mi Cuenta" },
                    new SystemSubArea { SystemAreaId = accountManagementAreaId, Name = "Clients" },
                    new SystemSubArea { SystemAreaId = accountManagementAreaId, Name = "Projects" },
                    new SystemSubArea { SystemAreaId = recruitingAreaId, Name = "Interviews" }
                };

                    foreach (var subArea in systemSubAreasList)
                    {
                        if (await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == subArea.Name) == null)
                        {
                            SystemSubArea ssa = new()
                            {
                                Name = subArea.Name,
                                SystemAreaId = subArea.SystemAreaId
                            };
                            await _db.SYSTEM_SUB_AREAS.AddAsync(ssa);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- CLAIMS --------------------------------

                    List<ApplicationSystemClaim> systemClaimsList = new List<ApplicationSystemClaim>();

                    // ADMIN CENTER
                    var softlandSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Actualizar Datos desde Softland");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = AdminCenterClaimsCD.Actualizar_Datos_Softland_ClaimType,
                        ClaimValue = AdminCenterClaimsCD.Actualizar_Datos_Softland_ClaimValue,
                        Description = "Acceso a poder actualizar los datos extraídos desde Softland",
                        SystemSubAreaId = softlandSubAreaId.SystemSubAreaId
                    });

                    var userRolesPermissionsSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Roles y Permisos de Usuarios");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimType,
                        ClaimValue = AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimValue,
                        Description = "Acceso a ver y editar los roles y permisos de usuarios",
                        SystemSubAreaId = userRolesPermissionsSubAreaId.SystemSubAreaId
                    });
                    var consultantPositionsAcConSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Consultant Positions Accounting Configuration");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = ConsultantPositionsClaimsCD.Manage_Consultant_Positions_ClaimType,
                        ClaimValue = ConsultantPositionsClaimsCD.Manage_Consultant_Positions_ClaimValue,
                        Description = "Have access to manage the consultant positions accounting configuration",
                        SystemSubAreaId = consultantPositionsAcConSubAreaId.SystemSubAreaId
                    });
                    // NOTES FOR ADMIN CENTER PERMISSIONS:
                    // Add every permission to the AnyOfPoliciesAdminCenterRequirementHandler

                    // FINANCES
                    var accountsReceivableSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Cuentas Por Cobrar");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = FinancesClaimsCD.Accounts_Receivable_ClaimType,
                        ClaimValue = FinancesClaimsCD.Accounts_Receivable_ClaimValue,
                        Description = "Acceso a la sección de cuentas por cobrar",
                        SystemSubAreaId = accountsReceivableSubAreaId.SystemSubAreaId
                    });

                    var financialCalculatorSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Calculadora Financiera");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = FinancesClaimsCD.Financial_Calculator_ClaimType,
                        ClaimValue = FinancesClaimsCD.Financial_Calculator_ClaimValue,
                        Description = "Acceso básico a la calculadora financiera",
                        SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                    });
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = FinancesClaimsCD.Financial_Calculator_BasicConfig_ClaimType,
                        ClaimValue = FinancesClaimsCD.Financial_Calculator_BasicConfig_ClaimValue,
                        Description = "Acceso a la configuración básica de la calculadora financiera",
                        SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                    });
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = FinancesClaimsCD.Financial_Calculator_AdvancedConfig_ClaimType,
                        ClaimValue = FinancesClaimsCD.Financial_Calculator_AdvancedConfig_ClaimValue,
                        Description = "Acceso avanzado en la configuración de la calculadora (editar los porcentages de utilidad, porcentages de riesgo, etc.)",
                        SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                    });
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = FinancesClaimsCD.Financial_Calculator_Profit_And_Details_ClaimType,
                        ClaimValue = FinancesClaimsCD.Financial_Calculator_Profit_And_Details_ClaimValue,
                        Description = "Acceso a ver las utilidades de los resultados de la calculadora financiera y a más detalles.",
                        SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                    });
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = FinancesClaimsCD.Financial_Calculator_Remove_Expenses_And_Costs_And_Edit_Vacations_ClaimType,
                        ClaimValue = FinancesClaimsCD.Financial_Calculator_Remove_Expenses_And_Costs_And_Edit_Vacations_ClaimValue,
                        Description = "Acceso a editar la opcion de vacaciones y remover gastos y costos para no ser tomados en cuenta en el calculo de la calculadora financiera.",
                        SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                    });

                    var paymentDebitsAndCreditsSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Consultant Payment Debits & Credits");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = FinancesClaimsCD.Manage_Payment_Debits_Credits_ClaimType,
                        ClaimValue = FinancesClaimsCD.Manage_Payment_Debits_Credits_ClaimValue,
                        Description = "Have access to manage payment debits and credits of payments to consultants.",
                        SystemSubAreaId = paymentDebitsAndCreditsSubAreaId.SystemSubAreaId
                    });

                    var paymentSheetsSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Payment Sheets");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = FinancesClaimsCD.Manage_Basic_Payment_Sheets_ClaimType,
                        ClaimValue = FinancesClaimsCD.Manage_Basic_Payment_Sheets_ClaimValue,
                        Description = "Have access to manage the basics of Payment Sheets.",
                        SystemSubAreaId = paymentSheetsSubAreaId.SystemSubAreaId
                    });

                    var exportAccountingDataSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Export Accounting Data");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = FinancesClaimsCD.Access_Export_Accounting_Data_ClaimType,
                        ClaimValue = FinancesClaimsCD.Access_Export_Accounting_Data_ClaimValue,
                        Description = "Have access to export the accounting data.",
                        SystemSubAreaId = exportAccountingDataSubAreaId.SystemSubAreaId
                    });

                    // GENERAL - CONSULTANTS
                    var consultantsSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Consultants");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = ConsultantsClaimsCD.Consultants_Page_ClaimType,
                        ClaimValue = ConsultantsClaimsCD.Consultants_Page_ClaimValue,
                        Description = "Access to manage only Computer Consultants (Developers, QAs...)",
                        SystemSubAreaId = consultantsSubAreaId.SystemSubAreaId
                    });
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType,
                        ClaimValue = ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue,
                        Description = "Access to manage all consultants, including Administrative Consultants",
                        SystemSubAreaId = consultantsSubAreaId.SystemSubAreaId
                    });

                    // GENERAL - HOLIDAYS
                    var holidaysSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Holidays");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = HolidaysClaimsCD.Holidays_Page_ClaimType,
                        ClaimValue = HolidaysClaimsCD.Holidays_Page_ClaimValue,
                        Description = "Acceso básico para ver todos los holidays",
                        SystemSubAreaId = holidaysSubAreaId.SystemSubAreaId
                    });

                    // GENERAL - CONSULTANT REIMBURSED BENEFITS
                    var consultantsBenefitsSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Consultant Reimbursed Benefits");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimType,
                        ClaimValue = ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimValue,
                        Description = "Access to manage the consultant reimbursed benefits to pay.",
                        SystemSubAreaId = consultantsBenefitsSubAreaId.SystemSubAreaId
                    });


                    // HOURS TRACKING TOOL
                    var reportingMyTimeSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Reporting My Time");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = TrackingToolClaimsCD.Reporting_My_Time_Basic_Access_ClaimType,
                        ClaimValue = TrackingToolClaimsCD.Reporting_My_Time_Basic_Access_ClaimValue,
                        Description = "Basic access to report their time",
                        SystemSubAreaId = reportingMyTimeSubAreaId.SystemSubAreaId
                    });
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = TrackingToolClaimsCD.General_Reports_ClaimType,
                        ClaimValue = TrackingToolClaimsCD.General_Reports_ClaimValue,
                        Description = "Access to view hours report for all consultants",
                        SystemSubAreaId = reportingMyTimeSubAreaId.SystemSubAreaId
                    });

                    // PROJECT MANAGEMENT - CLIENTS
                    var clientsSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Clients");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = ClientsClaimsCD.Clients_Page_ClaimType,
                        ClaimValue = ClientsClaimsCD.Clients_Page_ClaimValue,
                        Description = "Acces to view the Clients list",
                        SystemSubAreaId = clientsSubAreaId.SystemSubAreaId
                    });
                    // PROJECT MANAGEMENT - PROJECTS
                    var projectsSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Projects");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = ProjectsClaimsCD.Projects_Page_ClaimType,
                        ClaimValue = ProjectsClaimsCD.Projects_Page_ClaimValue,
                        Description = "Acces to view the Projects list",
                        SystemSubAreaId = projectsSubAreaId.SystemSubAreaId
                    });

                    // PROJECT MANAGEMENT - PROJECTS
                    var interviewsSubAreaId = await _db.SYSTEM_SUB_AREAS.FirstOrDefaultAsync(x => x.Name == "Interviews");
                    systemClaimsList.Add(new ApplicationSystemClaim
                    {
                        ClaimType = InterviewsClaimsCD.Manage_Interviews_Page_ClaimType,
                        ClaimValue = InterviewsClaimsCD.Manage_Interviews_ClaimValue,
                        Description = "Acces to manage Interviews",
                        SystemSubAreaId = interviewsSubAreaId.SystemSubAreaId
                    });

                    foreach (var claim in systemClaimsList)
                    {
                        if (await _db.APPLICATION_SYSTEM_CLAIMS.FirstOrDefaultAsync(x => x.ClaimType == claim.ClaimType && x.ClaimValue == claim.ClaimValue) == null)
                        {
                            ApplicationSystemClaim asc = new()
                            {
                                ClaimType = claim.ClaimType,
                                ClaimValue = claim.ClaimValue,
                                Description = claim.Description,
                                SystemSubAreaId = claim.SystemSubAreaId
                            };
                            await _db.APPLICATION_SYSTEM_CLAIMS.AddAsync(asc);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- USER ROLES --------------------------------

                    List<IdentityRole> rolesList = new List<IdentityRole>
                {
                    new IdentityRole { Name = SD.Role_User_Master },
                    new IdentityRole { Name = SD.Role_User_Admin },
                    new IdentityRole { Name = SD.Role_User_Computer_Consultant }
                };

                    foreach (var role in rolesList)
                    {
                        if (await _roleManager.FindByNameAsync(role.Name) == null)
                        {
                            await _roleManager.CreateAsync(role);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // ----------------- CREATE DEFAULT MASTER USER --------------------------------

                    var masterUsers = await _userManager.GetUsersInRoleAsync(SD.Role_User_Master);
                    if (masterUsers == null || !masterUsers.Any())
                    {
                        //Create user
                        var masterUserEmail = _config["MasterUserEmailENV"];
                        var masterUserPass = _config["MasterUserPassENV"];

                        var existingUser = await _userManager.FindByEmailAsync(masterUserEmail);

                        if (existingUser == null)
                        {
                            var userCategoryAdministrative = await _db.UserCategories.FirstOrDefaultAsync(x => x.Name == "Administrative");
                            var user = new ApplicationUser
                            {
                                UserName = masterUserEmail,
                                Email = masterUserEmail,
                                Name = _config["MasterUserNameEnv"],
                                LastName = _config["MasterUserLastNameEnv"],
                                IsActive = true,
                                DeactivationDate = null,
                                UserCategoryId = userCategoryAdministrative.UserCategoryId,
                                PhoneNumberConfirmed = false,
                                TwoFactorEnabled = false,
                                LockoutEnabled = true,
                                AccessFailedCount = 0,
                                TwoFactorRequired = true,
                                EmailConfirmed = true
                            };
                            await _userManager.CreateAsync(user, masterUserPass);
                            var createdUser = await _db.AspNetUsers.FirstOrDefaultAsync(x => x.Email == masterUserEmail);
                            createdUser.Name = "Master User Name";
                            createdUser.LastName = "Master User Last Name";

                            if (createdUser != null)
                            {
                                await _userManager.AddToRoleAsync(createdUser, SD.Role_User_Master);
                            }
                            await _db.SaveChangesAsync();

                            //Create Consultant Details
                            var defaultPaymentMethod = await _db.PAYMENT_METHODS.FirstOrDefaultAsync(x => x.Name == "Bac Credomatic different from Panamá (Ameritransfer)");
                            if (defaultPaymentMethod == null)
                            {
                                throw new Exception();
                            }

                            var consultantDetailToCreate = new ConsultantDetail()
                            {
                                UserId = createdUser.Id,
                                CreationDate = DateTime.UtcNow,
                                IdCountry = "CRI",
                                UserCreatedBy = createdUser.Id,
                                CompanyId = "OCE",
                                PaymentMethodId = defaultPaymentMethod.PaymentMethodId,
                                PaymentPeriod = 1,
                                StartDate = DateTime.UtcNow,
                                WorkingModel = 1
                            };
                            var userActiveHistoryToCreate = new ApplicationUserActiveHistory()
                            {
                                UserId = createdUser.Id,
                                IsActive = true,
                                ActionDate = DateTime.UtcNow,
                                UserIdActionedBy = createdUser.Id
                            };
                            await _db.CONSULTANT_DETAILS.AddAsync(consultantDetailToCreate);
                            await _db.UsersActiveHistory.AddAsync(userActiveHistoryToCreate);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"The user with the email: '{masterUserEmail}' already exists but is not a master user.");
                            Console.ResetColor();
                            throw new InvalidOperationException("User exists but is not assigned to Role_User_Master.");
                        }
                    }
                    // ----------------- ROLE CLAIMS --------------------------------

                    // Obtain the MASTER role
                    var masterRole = await _roleManager.FindByNameAsync(SD.Role_User_Master);

                    // Get the first user who has that master role
                    var firstMasterUserId = await _db.UserRoles
                        .Where(ur => ur.RoleId == masterRole.Id)
                        .Select(ur => ur.UserId)
                        .FirstOrDefaultAsync();

                    List<ApplicationRoleClaim> roleClaimsList = new List<ApplicationRoleClaim>();

                    // Generate the new role claims
                    foreach (var systemClaim in systemClaimsList)
                    {
                        // Check if the role claim already exists
                        bool claimExists = await _db.RoleClaims.AnyAsync(rc =>
                            rc.RoleId == masterRole.Id &&
                            rc.ClaimType == systemClaim.ClaimType &&
                            rc.ClaimValue == systemClaim.ClaimValue);

                        if (!claimExists)
                        {
                            roleClaimsList.Add(new ApplicationRoleClaim
                            {
                                RoleId = masterRole.Id,
                                ClaimType = systemClaim.ClaimType,
                                ClaimValue = systemClaim.ClaimValue,
                                CreatedBy = firstMasterUserId,
                                CreationDate = DateTime.UtcNow
                            });
                        }
                    }

                    // Save to database
                    if (roleClaimsList.Any())
                    {
                        await _db.RoleClaims.AddRangeAsync(roleClaimsList);
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error during database initialization:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.ResetColor();
                    throw new InvalidOperationException("Error during database initialization.");
                }
            }
        }
    }
}
