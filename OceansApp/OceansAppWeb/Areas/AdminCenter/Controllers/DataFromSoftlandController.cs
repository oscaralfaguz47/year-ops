using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Cors;
using OceansApp.Models.ViewModels.DataFromSoftland;

namespace OceansApp.Areas.Admin.Controllers
{
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    [Authorize(Policy = "AccessToUpdateDataFromSoftlandSection")]
    public class DataFromSoftlandController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public DataFromSoftlandController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            DataUpdateDate dateLastGlobalUpdate = await _unitOfWork.DataUpdateDates.GetLastDate();

            if (dateLastGlobalUpdate != null)
            {
                TempData["globalLastDate"] = dateLastGlobalUpdate.Date;
                TempData["globalLastDateSection"] = dateLastGlobalUpdate.SectionsUpdated;
            }
            else
            {
                TempData["globalLastDate"] = DateTime.Now;
                TempData["globalLastDateSection"] = "There are no records yet";
            }

            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateData(DataFromSoftland obj)
        {
            try
            {
                var updatedRecords = 0;
                if (ModelState.IsValid)
                {
                    if (isValidJSON(obj.DataToSave))
                    {
                        dynamic jsonFromInput = JsonConvert.DeserializeObject(obj.DataToSave);

                        if (validateCorrectJsonStructureAccountingAccount(obj.DataToSave) &&
                            validateCorrectJsonStructureCostCenter(obj.DataToSave)
                            && validateCorrectJsonStructureLedgerMovement(obj.DataToSave)
                            && validateCorrectJsonStructureClients(obj.DataToSave)
                            && validateCorrectJsonStructureCountry(obj.DataToSave)
                            && validateCorrectJsonStructureDocumentsCC(obj.DataToSave)
                            && validateCorrectJsonStructureCostCenterAccount(obj.DataToSave)
                            && validateCorrectJsonStructureBankAccount(obj.DataToSave))
                        {
                            var updatedSections = "";

                            // INSERT ACCOUNTING ACCOUNTS
                            if (jsonFromInput.accountingAccounts != null)
                            {
                                int affectedRecords = 0;
                                foreach (var jsonMaster in jsonFromInput.accountingAccounts)
                                {
                                    AccountingAccount accountingAccount = new()
                                    {
                                        AccountingAccountCode = jsonMaster.CUENTA_CONTABLE,
                                        Description = jsonMaster.DESCRIPCION,
                                        DescriptionIFRS = jsonMaster.DESCRIPCION_IFRS,
                                        AccountingAccountType = jsonMaster.TIPO,
                                        DetailedType = jsonMaster.TIPO_DETALLADO,
                                        Balance = jsonMaster.SALDO_NORMAL,
                                        AcceptData = jsonMaster.ACEPTA_DATOS,
                                        UseCostCenter = jsonMaster.USA_CENTRO_COSTO,
                                        UseThird = jsonMaster.MANEJA_TERCERO,
                                        DateLastUpdate = jsonMaster.FCH_HORA_ULT_MOD,
                                        DateHour = jsonMaster.FECHA_HORA,
                                        CompanyId = jsonMaster.CompanyId
                                    };
                                    if (await _unitOfWork.AccountingAccounts.UpdateIfExistAddIfNot(accountingAccount))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                    }
                                    await _unitOfWork.SaveAsync();
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Accounting Accounts /";
                                }
                                updatedRecords = updatedRecords + affectedRecords;
                            }

                            //INSERT COSTS OF CENTER OF COSTS
                            if (jsonFromInput.centerOfCosts != null)
                            {
                                int affectedRecords = 0;
                                foreach (var jsonMaster in jsonFromInput.centerOfCosts)
                                {
                                    CostCenter costCenter = new()
                                    {
                                        CostCenterCode = jsonMaster.CENTRO_COSTO,
                                        Description = jsonMaster.DESCRIPCION,
                                        AcceptData = jsonMaster.ACEPTA_DATOS,
                                        CreateDate = jsonMaster.CreateDate,
                                        CompanyId = jsonMaster.CompanyId
                                    };
                                    if (await _unitOfWork.CenterOfCosts.UpdateIfExistAddIfNot(costCenter))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                    }
                                    await _unitOfWork.SaveAsync();
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Centers of costs /";
                                }
                                updatedRecords = updatedRecords + affectedRecords;
                            }
                            //INSERT LEDGER MOVEMENTS
                            if (jsonFromInput.ledgerMovements != null)
                            {
                                var ledgerMovementsList = new List<CreateLedgerMovementVM>();

                                foreach (var jsonMaster in jsonFromInput.ledgerMovements)
                                {
                                    var accountingAccountCode = (string?)jsonMaster.CUENTA_CONTABLE ?? string.Empty;
                                    var costCenterCode = (string?)jsonMaster.CENTRO_COSTO ?? string.Empty;
                                    var companyId = (string?)jsonMaster.CompanyId ?? string.Empty;

                                    if (string.IsNullOrWhiteSpace(accountingAccountCode) && string.IsNullOrWhiteSpace(companyId))
                                    {
                                        companyId = string.Empty;
                                    }

                                    ledgerMovementsList.Add(new CreateLedgerMovementVM
                                    {
                                        IdSeat = jsonMaster.ASIENTO ?? string.Empty,
                                        Consecutive = jsonMaster.CONSECUTIVO,
                                        Date = jsonMaster.FECHA,
                                        LocalDebit = jsonMaster.DEBITO_LOCAL,
                                        LocalCredit = jsonMaster.CREDITO_LOCAL,
                                        AccountingType = jsonMaster.CONTABILIDAD ?? string.Empty,
                                        RecordDate = jsonMaster.RecordDate,
                                        AccountingAccountCode = accountingAccountCode,
                                        CompanyId = companyId,
                                        CostCenterCode = costCenterCode
                                    });
                                }

                                int affectedRecords = await _unitOfWork.LedgerMovements.AddIfNotExistBulkAsync(ledgerMovementsList);

                                if (affectedRecords > 0)
                                {
                                    updatedSections += "Ledger Movements /";
                                }
                                updatedRecords = affectedRecords;
                            }

                            //INSERT CLIENTS
                            if (jsonFromInput.clients != null)
                            {
                                int affectedRecords = 0;
                                foreach (var jsonMaster in jsonFromInput.clients)
                                {
                                    DateTime? lastUpdate = null;
                                    if (jsonMaster.FCH_HORA_ULT_MOD != "")
                                    {
                                        lastUpdate = jsonMaster.FCH_HORA_ULT_MOD;
                                    }
                                    Client client = new()
                                    {
                                        ClientCode = jsonMaster.CLIENTE,
                                        Name = jsonMaster.NOMBRE,
                                        Alias = jsonMaster.ALIAS,
                                        Contact = jsonMaster.CONTACTO,
                                        ContactOccupation = jsonMaster.CARGO,
                                        Phone1 = jsonMaster.TELEFONO1,
                                        Phone2 = jsonMaster.TELEFONO2,
                                        AdmissionDate = jsonMaster.FECHA_INGRESO,
                                        PaymentCondition = jsonMaster.CONDICION_PAGO,
                                        Discount = jsonMaster.DESCUENTO,
                                        IsActive = jsonMaster.ACTIVO,
                                        ClientCategory = jsonMaster.CATEGORIA_CLIENTE,
                                        ClientClass = jsonMaster.CLASE_ABC,
                                        Emails = jsonMaster.E_MAIL,
                                        Notes = jsonMaster.NOTAS,
                                        DateLastUpdate = lastUpdate,
                                        Address = jsonMaster.OTRAS_SENAS,
                                        CreationDate = jsonMaster.CreateDate,
                                        CompanyId = jsonMaster.CompanyId,
                                        LatePaymentFee = 0,
                                        AllowSentLatePaymentNotifications = true
                                    };
                                    if (await _unitOfWork.Client.UpdateIfExistAddIfNot(client))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                        await _unitOfWork.SaveAsync();
                                    }
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Clients /";
                                }
                                updatedRecords = updatedRecords + affectedRecords;
                            }

                            //INSERT COUNTRIES
                            if (jsonFromInput.countries != null)
                            {
                                int affectedRecords = 0;
                                foreach (var jsonMaster in jsonFromInput.countries)
                                {
                                    Country country = new()
                                    {
                                        IdCountry = jsonMaster.PAIS,
                                        Name = jsonMaster.NOMBRE,
                                        CreateDate = jsonMaster.CreateDate
                                    };
                                    if (await _unitOfWork.Country.UpdateIfExistAddIfNot(country))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                    }
                                    await _unitOfWork.SaveAsync();
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Countries /";
                                }
                                updatedRecords = updatedRecords + affectedRecords;
                            }

                            //INSERT DOCUMENTS CC
                            if (jsonFromInput.documentsCC != null)
                            {
                                int affectedRecords = 0;
                                foreach (var jsonMaster in jsonFromInput.documentsCC)
                                {
                                    DateTime? lastUpdate = null;
                                    if (jsonMaster.FECHA_ULT_MOD != "")
                                    {
                                        lastUpdate = jsonMaster.FECHA_ULT_MOD;
                                    }
                                    var clientCode = "";
                                    var companyId = "";
                                    if (jsonMaster.CLIENTE != null)
                                    {
                                        clientCode = jsonMaster.CLIENTE;
                                        companyId = jsonMaster.CompanyId;
                                    }
                                    var client = await _unitOfWork.Client.GetFirstOrDefaultAsync(x => x.ClientCode == clientCode
                                    && x.CompanyId == companyId);

                                    DocumentCC document = new()
                                    {
                                        DocumentNumber = jsonMaster.DOCUMENTO,
                                        DocumentType = jsonMaster.TIPO,
                                        ApplicationDescription = jsonMaster.APLICACION,
                                        DocumentDate = jsonMaster.FECHA_DOCUMENTO,
                                        DocumentAmount = jsonMaster.MONTO,
                                        BalanceAmount = jsonMaster.SALDO,
                                        Canceled = jsonMaster.ANULADO,
                                        IdSeat = jsonMaster.ASIENTO,
                                        ClientId = client.ClientId,
                                        DateLastUpdate = lastUpdate,
                                        CreationDate = jsonMaster.CreateDate,
                                        CompanyId = companyId
                                    };
                                    if (await _unitOfWork.DocumentCC.UpdateIfExistAddIfNot(document))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                        await _unitOfWork.SaveAsync();
                                    }
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Documents CC /";
                                }
                                updatedRecords = updatedRecords + affectedRecords;
                            }

                            //INSERT COST CENTER ACCOUNTING ACCOUNT
                            if (jsonFromInput.costsCenterAccounts != null)
                            {
                                int affectedRecords = 0;
                                foreach (var jsonMaster in jsonFromInput.costsCenterAccounts)
                                {
                                    var costCenterCode = "";
                                    var accountingAccountCode = "";
                                    var companyId = "";
                                    if (jsonMaster.CENTRO_COSTO != null)
                                    {
                                        costCenterCode = jsonMaster.CENTRO_COSTO;
                                        accountingAccountCode = jsonMaster.CUENTA_CONTABLE;
                                        companyId = jsonMaster.CompanyId;
                                    }
                                    var costCenter = await _unitOfWork.CenterOfCosts.GetFirstOrDefaultAsync(x => x.CostCenterCode == costCenterCode
                                    && x.CompanyId == companyId);
                                    var accountingAccount = await _unitOfWork.AccountingAccounts.GetFirstOrDefaultAsync(x => x.AccountingAccountCode == accountingAccountCode
                                    && x.CompanyId == companyId);

                                    CostCenterAccountingAccount costCenterAccount = new()
                                    {
                                        CostCenterId = costCenter.CostCenterId,
                                        AccountingAccountId = accountingAccount.AccountingAccountId,
                                        Status = jsonMaster.ESTADO,
                                        CreateDate = jsonMaster.CreateDate,
                                        CompanyId = companyId
                                    };
                                    if (await _unitOfWork.CostCenterAccountingAccount.AddCostCenterAccountingAccount(costCenterAccount))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                        await _unitOfWork.SaveAsync();
                                    }
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Costs Centes Accounting Accounts /";
                                }
                                updatedRecords = updatedRecords + affectedRecords;
                            }

                            //INSERT BANK ACCOUNTS
                            if (jsonFromInput.bankAccounts != null)
                            {
                                int affectedRecords = 0;
                                foreach (var jsonMaster in jsonFromInput.bankAccounts)
                                {
                                    BankAccount bankAccount = new()
                                    {
                                        BankAccountCode = jsonMaster.CUENTA_BANCO,
                                        BankAccountName = jsonMaster.NOMBRE,
                                        CompanyId = jsonMaster.CompanyId,
                                        IsActive = jsonMaster.ACTIVA
                                    };
                                    if (await _unitOfWork.BankAccount.AddBankAccount(bankAccount))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                        await _unitOfWork.SaveAsync();
                                    }
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Bank Accounts /";
                                }
                                updatedRecords = updatedRecords + affectedRecords;
                            }

                            if (updatedSections != "")
                            {
                                //INSERT DATE
                                var claimsIdentity = (ClaimsIdentity)User.Identity;
                                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                                DataUpdateDate dataUpdateDate = new()
                                {
                                    SectionsUpdated = updatedSections,
                                    CreatedBy = claim.Value
                                };
                                await _unitOfWork.DataUpdateDates.AddAsync(dataUpdateDate);
                                await _unitOfWork.SaveAsync();
                            }
                            TempData["success"] = updatedRecords + " records were affected.";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            if (!validateCorrectJsonStructureAccountingAccount(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "The JSON structure is not correct for Accounting Accounts");
                            }
                            if (!validateCorrectJsonStructureCostCenter(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "The JSON structure is not correct for Cost Centers");
                            }
                            if (!validateCorrectJsonStructureLedgerMovement(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "The JSON structure is not correct for the Mayor's movements");
                            }
                            if (!validateCorrectJsonStructureClients(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "The JSON structure is not correct for Clients");
                            }
                            if (!validateCorrectJsonStructureCountry(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "The JSON structure is not correct for countries");
                            }
                            if (!validateCorrectJsonStructureDocumentsCC(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "The JSON structure is not correct for CC Documents");
                            }
                            if (!validateCorrectJsonStructureCostCenterAccount(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "The JSON structure is not correct for Costs Centers Accounting Accounts");
                            }
                            if (!validateCorrectJsonStructureBankAccount(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "The JSON structure is not correct for Bank Accounts");
                            }

                            return View("Index");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("dataToSave", "The data to be included must be valid JSON");
                        return View("Index");
                    }
                }
                return View("Index");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        public bool isValidJSON(String json)
        {
            try
            {
                var jsonObject = JsonValue.Parse(json);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool validateCorrectJsonStructureAccountingAccount(String jsonString)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(jsonString);
                if (json.accountingAccounts != null)
                {
                    foreach (var result in json.accountingAccounts)
                    {
                        AccountingAccount accountingAccount = new()
                        {
                            AccountingAccountCode = result.CUENTA_CONTABLE,
                            Description = result.DESCRIPCION,
                            DescriptionIFRS = result.DESCRIPCION_IFRS,
                            AccountingAccountType = result.TIPO,
                            DetailedType = result.TIPO_DETALLADO,
                            Balance = result.SALDO_NORMAL,
                            AcceptData = result.ACEPTA_DATOS,
                            UseCostCenter = result.USA_CENTRO_COSTO,
                            UseThird = result.MANEJA_TERCERO,
                            DateLastUpdate = result.FCH_HORA_ULT_MOD,
                            DateHour = result.FECHA_HORA
                        };
                        if (accountingAccount.AccountingAccountCode == null || accountingAccount.Description == null || accountingAccount.AccountingAccountType == null
                        || accountingAccount.DetailedType == null || accountingAccount.Balance == null || accountingAccount.AcceptData == null
                        || accountingAccount.UseCostCenter == null || accountingAccount.UseThird == null || accountingAccount.DateLastUpdate.ToString() == null
                        || accountingAccount.DateHour.ToString() == null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return true;
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool validateCorrectJsonStructureCostCenter(String jsonString)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(jsonString);


                if (json.centerOfCosts != null)
                {
                    foreach (var result in json.centerOfCosts)
                    {
                        CostCenter costCenter = new()
                        {
                            CostCenterCode = result.CENTRO_COSTO,
                            Description = result.DESCRIPCION,
                            AcceptData = result.ACEPTA_DATOS,
                            CreateDate = result.CreateDate,
                            CompanyId = result.CompanyId
                        };
                        if (costCenter.CostCenterCode == null || costCenter.Description == null || costCenter.AcceptData == null
                            || costCenter.CreateDate.ToString() == null || costCenter.CompanyId == null)
                        {
                            return false;
                        }
                    }

                }
                else
                {
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool validateCorrectJsonStructureLedgerMovement(String jsonString)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(jsonString);

                if (json.ledgerMovements != null)
                {
                    foreach (var result in json.ledgerMovements)
                    {
                        LedgerMovement ledgerMovement = new()
                        {
                            IdSeat = result.ASIENTO,
                            Consecutive = result.CONSECUTIVO,
                            Date = result.FECHA,
                            LocalDebit = result.DEBITO_LOCAL,
                            LocalCredit = result.CREDITO_LOCAL,
                            AccountingType = result.CONTABILIDAD,
                            RecordDate = result.RecordDate,
                            CompanyId = result.CompanyId
                        };
                        if (ledgerMovement.IdSeat == null || ledgerMovement.Consecutive.ToString() == null
                            || ledgerMovement.Date.ToString() == null
                            || ledgerMovement.LocalDebit.ToString() == null || ledgerMovement.LocalCredit.ToString() == null
                            || ledgerMovement.AccountingType == null || ledgerMovement.RecordDate.ToString() == null || ledgerMovement.CompanyId == null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool validateCorrectJsonStructureClients(String jsonString)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(jsonString);

                if (json.clients != null)
                {
                    foreach (var result in json.clients)
                    {
                        DateTime? dateLastUpdate = null;
                        if (result.FCH_HORA_ULT_MOD != "")
                        {
                            dateLastUpdate = result.FCH_HORA_ULT_MOD;
                        }
                        Client client = new()
                        {
                            ClientCode = result.CLIENTE,
                            Name = result.NOMBRE,
                            Alias = result.ALIAS,
                            Contact = result.CONTACTO,
                            ContactOccupation = result.CARGO,
                            Phone1 = result.TELEFONO1,
                            Phone2 = result.TELEFONO2,
                            AdmissionDate = result.FECHA_INGRESO,
                            PaymentCondition = result.CONDICION_PAGO,
                            Discount = result.DESCUENTO,
                            IsActive = result.ACTIVO,
                            ClientCategory = result.CATEGORIA_CLIENTE,
                            ClientClass = result.CLASE_ABC,
                            Emails = result.E_MAIL,
                            Notes = result.NOTAS,
                            DateLastUpdate = dateLastUpdate,
                            Address = result.OTRAS_SENAS,
                            CreationDate = result.CreateDate,
                            CompanyId = result.CompanyId
                        };
                        if (client.ClientCode == null || client.Name == null || client.AdmissionDate.ToString() == null
                            || client.PaymentCondition == null || client.Discount.ToString() == null
                            || client.IsActive == null || client.ClientCategory == null
                            || client.CreationDate.ToString() == null || client.CompanyId == null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool validateCorrectJsonStructureCountry(String jsonString)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(jsonString);

                if (json.countries != null)
                {
                    foreach (var result in json.countries)
                    {
                        Country country = new()
                        {
                            IdCountry = result.PAIS,
                            Name = result.NOMBRE,
                            CreateDate = result.CreateDate
                        };
                        if (country.IdCountry == null || country.Name == null
                            || country.CreateDate.ToString() == null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool validateCorrectJsonStructureDocumentsCC(String jsonString)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(jsonString);

                if (json.documentsCC != null)
                {
                    foreach (var result in json.documentsCC)
                    {
                        DateTime? dateLastUpdate = null;
                        if (result.FECHA_ULT_MOD != "")
                        {
                            dateLastUpdate = result.FECHA_ULT_MOD;
                        }
                        DocumentCC document = new()
                        {
                            DocumentNumber = result.DOCUMENTO,
                            DocumentType = result.TIPO,
                            DocumentDate = result.FECHA_DOCUMENTO,
                            DocumentAmount = result.MONTO,
                            BalanceAmount = result.SALDO,
                            Canceled = result.ANULADO,
                            DateLastUpdate = dateLastUpdate,
                            CreationDate = result.CreateDate,
                            CompanyId = result.CompanyId
                        };
                        if (document.DocumentNumber == null || document.DocumentType == null || document.DocumentDate.ToString() == null
                            || document.DocumentAmount == null || document.BalanceAmount == null
                            || document.Canceled == null || document.CreationDate.ToString() == null || document.CompanyId == null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool validateCorrectJsonStructureCostCenterAccount(String jsonString)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(jsonString);

                if (json.costsCenterAccounts != null)
                {
                    foreach (var result in json.costsCenterAccounts)
                    {
                        CostCenterAccountingAccount costCenterAccount = new()
                        {
                            Status = result.ESTADO,
                            CreateDate = result.CreateDate,
                            CompanyId = result.CompanyId
                        };
                        if (costCenterAccount.Status == null
                            || costCenterAccount.CompanyId == null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool validateCorrectJsonStructureBankAccount(String jsonString)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(jsonString);

                if (json.bankAccounts != null)
                {
                    foreach (var result in json.bankAccounts)
                    {
                        BankAccount bankAccount = new()
                        {
                            BankAccountCode = result.CUENTA_BANCO,
                            BankAccountName = result.NOMBRE,
                            CompanyId = result.CompanyId,
                            IsActive = result.ACTIVA
                        };
                        if (bankAccount.BankAccountCode == null
                            || bankAccount.CompanyId == null || bankAccount.BankAccountName == null || bankAccount.IsActive == null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

}
