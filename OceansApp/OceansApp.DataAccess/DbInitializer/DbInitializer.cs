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
using System.Linq;

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
            bool isThereNewMigrationToUpdate = true; // False if no migration updates in the DB are needed
            if (isThereNewMigrationToUpdate)
            {
                // Migrations if they are not applied
                try
                {
                    if ((await _db.Database.GetPendingMigrationsAsync()).Any())
                    {
                        await _db.Database.MigrateAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle exception as needed
                }
            }

            bool createDefaultDataToDatabase = true; // False if no updates in the DB are needed

            if (createDefaultDataToDatabase)
            {
                // ----------------- ROLES --------------------------------

                List<IdentityRole> rolesList = new List<IdentityRole>
                {
                    new IdentityRole { Name = SD.Role_User_Master },
                    new IdentityRole { Name = SD.Role_User_Admin },
                    new IdentityRole { Name = SD.Role_User_Simple },
                    new IdentityRole { Name = SD.Role_User_Computer_Consultant }
                };

                foreach (var role in rolesList)
                {
                    if (await _roleManager.FindByNameAsync(role.Name) == null)
                    {
                        await _roleManager.CreateAsync(role);
                    }
                }

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
                    await _db.SaveChangesAsync();
                }

                // ----------------- CREATE DEFAULT USER --------------------------------

                if (!await _roleManager.RoleExistsAsync(SD.Role_User_Master))
                {
                    var masterUserEmail = Environment.GetEnvironmentVariable(_config["MasterUserEmail"]);
                    var masterUserPass = Environment.GetEnvironmentVariable(_config["MasterUserPass_ENV"]);

                    // If Roles are not created, then we will create Master user as well
                    var user = new ApplicationUser
                    {
                        UserName = masterUserEmail,
                        Email = masterUserEmail,
                        Name = _config["MasterUserName"],
                        LastName = _config["MasterUserLastName"],
                        IsActive = true,
                        DeactivationDate = null
                    };
                    await _userManager.CreateAsync(user, masterUserPass);
                    var createdUser = await _db.AspNetUsers.FirstOrDefaultAsync(x => x.Email == masterUserEmail);

                    if (createdUser != null)
                    {
                        await _userManager.AddToRoleAsync(createdUser, SD.Role_User_Master);
                    }
                }

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

                // ----------------- CONSULTANT BENEFITS COMPANIES --------------------------------

                List<ConsultantBenefitCompany> consultantBenefitCompaniesList = new List<ConsultantBenefitCompany>();
                var peopleAndCultureCostCenterOCE = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "10-02-04" && x.CompanyId == "OCE");
                var peopleAndCultureCostCenterLLC = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "10-02-04" && x.CompanyId == "LLC");
                var accountingAccountReservaBalanceProgramOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "3-02-01-000-000" && x.CompanyId == "OCE");
                var accountingAccountReservaBonuslyOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "3-02-02-000-000" && x.CompanyId == "OCE");
                var accountingAccountOceansChallengeOCE = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode == "6-01-03-005-000" && x.CompanyId == "OCE");
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
                    x.CostCenterId == benefitCompany.CostCenterId && x.AccountingAccountId == benefitCompany.AccountingAccountId);
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

                // ----------------- GLOBAL CONSECUTIVES --------------------------------

                List<GlobalConsecutive> globalConsecutivesList = new List<GlobalConsecutive>
                {
                    new GlobalConsecutive { Name = "JOURNAL_CXP", ConsecutiveNumber = 0, CompanyId = "OCE" },
                    new GlobalConsecutive { Name = "JOURNAL_CXP", ConsecutiveNumber = 0, CompanyId = "LLC" }
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

                // ----------------- NOTIFICATIONS MEDIA --------------------------------

                List<NotificationMedia> notificatinMediaList = new List<NotificationMedia>
                {
                    new NotificationMedia { Name = "Email" },
                    new NotificationMedia { Name = "Slack" }
                };

                foreach (var notMedia in notificatinMediaList)
                {
                    var existingMedia = await _db.NOTIFICATION_MEDIA.FirstOrDefaultAsync(x => x.Name == notMedia.Name);
                    if (existingMedia == null)
                    {
                        NotificationMedia notificationMedia = new()
                        {
                            Name = notMedia.Name
                        };
                        await _db.NOTIFICATION_MEDIA.AddAsync(notificationMedia);
                    }
                }
                await _db.SaveChangesAsync();

                // ----------------- NOTIFICATION STATUS --------------------------------

                List<NotificationStatus> notificatinStatusList = new List<NotificationStatus>
                {
                    new NotificationStatus { Name = "Enviando" },
                    new NotificationStatus { Name = "Enviado" },
                    new NotificationStatus { Name = "No enviado" },
                    new NotificationStatus { Name = "Envío fallido" }
                };
                foreach (var notStatus in notificatinStatusList)
                {
                    var existingNS = await _db.NOTIFICATION_STATUS.FirstOrDefaultAsync(x => x.Name == notStatus.Name);
                    if (existingNS == null)
                    {
                        NotificationStatus notificationStatus = new()
                        {
                            Name = notStatus.Name
                        };
                        await _db.NOTIFICATION_STATUS.AddAsync(notificationStatus);
                    }
                }
                await _db.SaveChangesAsync();

                // ----------------- NOTIFICATION TYPES --------------------------------

                List<NotificationType> notificationTypeList = new List<NotificationType>
                {
                    new NotificationType { Name = "Cuentas por cobrar" },
                    new NotificationType { Name = "Create new Consultant" }
                };
                foreach (var notType in notificationTypeList)
                {
                    if (await _db.NOTIFICATION_TYPES.FirstOrDefaultAsync(x => x.Name == notType.Name) == null)
                    {
                        NotificationType notificationType = new()
                        {
                            Name = notType.Name
                        };
                        await _db.NOTIFICATION_TYPES.AddAsync(notificationType);
                    }
                }
                await _db.SaveChangesAsync();


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

                // ----------------- SYSTEM AREAS --------------------------------

                List<SystemArea> systemAreasList = new List<SystemArea>
                {
                    new SystemArea { Name = "Admin Center" },
                    new SystemArea { Name = "Finanzas" },
                    new SystemArea { Name = "General" },
                    new SystemArea { Name = "Tracking Tool" },
                    new SystemArea { Name = "Dashboard" },
                    new SystemArea { Name = "Mi Cuenta" },
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

                List<SystemSubArea> systemSubAreasList = new List<SystemSubArea>
                {
                    new SystemSubArea { SystemAreaId = 1, Name = "Actualizar Datos desde Softland" },
                    new SystemSubArea { SystemAreaId = 1, Name = "Roles y Permisos de Usuarios" },
                    new SystemSubArea { SystemAreaId = 1, Name = "Consultant Positions Accounting Configuration" },
                    new SystemSubArea { SystemAreaId = 2, Name = "Cuentas Por Cobrar" },
                    new SystemSubArea { SystemAreaId = 2, Name = "Consultant Payment Debits & Credits" },
                    new SystemSubArea { SystemAreaId = 2, Name = "Payment Sheets" },
                    new SystemSubArea { SystemAreaId = 2, Name = "Export Accounting Data" },
                    new SystemSubArea { SystemAreaId = 2, Name = "Calculadora Financiera" },
                    new SystemSubArea { SystemAreaId = 3, Name = "Consultants" },
                    new SystemSubArea { SystemAreaId = 3, Name = "Consultant Reimbursed Benefits" },
                    new SystemSubArea { SystemAreaId = 3, Name = "Holidays" },
                    new SystemSubArea { SystemAreaId = 4, Name = "Reporting My Time" },
                    new SystemSubArea { SystemAreaId = 5, Name = "Dashboard" },
                    new SystemSubArea { SystemAreaId = 6, Name = "Mi Cuenta" },
                    new SystemSubArea { SystemAreaId = 7, Name = "Clients" },
                    new SystemSubArea { SystemAreaId = 7, Name = "Projects" },
                    new SystemSubArea { SystemAreaId = 11, Name = "Interviews" }
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
                        await _db.SaveChangesAsync();
                    }
                }

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
                        PaymentMethodId = paymentMethods.FirstOrDefault(x => x.Name == "Payoneer").PaymentMethodId,
                        BankAccountId = bankAccounts.FirstOrDefault(x => x.BankAccountCode == "113454904").BankAccountId,
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
                    },
                    new PaymentMethodBankAccount {
                        PaymentMethodId = paymentMethods.FirstOrDefault(x => x.Name == "Wise").PaymentMethodId,
                        BankAccountId = bankAccounts.FirstOrDefault(x => x.BankAccountCode == "9600012642438917").BankAccountId,
                        IsDefault = true
                    },
                    new PaymentMethodBankAccount {
                        PaymentMethodId = paymentMethods.FirstOrDefault(x => x.Name == "Bac Credomatic Costa Rica (Bac CR to Bac CR)").PaymentMethodId,
                        BankAccountId = bankAccounts.FirstOrDefault(x => x.BankAccountCode == "947729737").BankAccountId,
                        IsDefault = true
                    },
                    new PaymentMethodBankAccount {
                        PaymentMethodId = paymentMethods.FirstOrDefault(x => x.Name == "Bac Credomatic Costa Rica (Bac CR to Bac CR)").PaymentMethodId,
                        BankAccountId = bankAccounts.FirstOrDefault(x => x.BankAccountCode == "951381904").BankAccountId,
                        IsDefault = false
                    },
                    new PaymentMethodBankAccount {
                        PaymentMethodId = paymentMethods.FirstOrDefault(x => x.Name == "USA local transfer").PaymentMethodId,
                        BankAccountId = bankAccounts.FirstOrDefault(x => x.BankAccountCode == "610851399").BankAccountId,
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
                    new ReportingMyTimeMovementType { Name = "Holidays", IsPayable = true }
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
                    new TransactionStatus { Name = "Accounted" }
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
            }
        }
    }
}
