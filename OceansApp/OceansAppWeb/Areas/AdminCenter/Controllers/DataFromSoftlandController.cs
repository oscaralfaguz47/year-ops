using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;

namespace OceansApp.Areas.Admin.Controllers
{
    [Area("AdminCenter")]
    [Authorize(Policy = "AccessToUpdateDataFromSoftlandSection")]
    public class DataFromSoftlandController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public DataFromSoftlandController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [RequireTwoFactorEnabled]
        public IActionResult Index()
        {
            DataUpdateDate dateLastGlobalUpdate = _unitOfWork.DataUpdateDates.GetLastDate();

            if (dateLastGlobalUpdate != null)
            {
                TempData["globalLastDate"] = dateLastGlobalUpdate.Date;
                TempData["globalLastDateSection"] = dateLastGlobalUpdate.SectionsUpdated;
            }
            else
            {
                TempData["globalLastDate"] = DateTime.Now;
                TempData["globalLastDateSection"] = "Aún no existen registros";
            }

            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateData(DataFromSoftland obj)
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
                            && validateCorrectJsonStructureProviderCategory(obj.DataToSave)
                            && validateCorrectJsonStructureCountry(obj.DataToSave)
                            && validateCorrectJsonStructureProvider(obj.DataToSave)
                            && validateCorrectJsonStructureDocumentsCC(obj.DataToSave))
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
                                    if (_unitOfWork.AccountingAccounts.UpdateIfExistAddIfNot(accountingAccount))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                    }
                                    _unitOfWork.Save();
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Cuentas Contables /";
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
                                    if (_unitOfWork.CenterOfCosts.UpdateIfExistAddIfNot(costCenter))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                    }
                                    _unitOfWork.Save();
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Centros de Costo /";
                                }
                                updatedRecords = updatedRecords + affectedRecords;
                            }
                            //INSERT LEDGER MOVEMENTS
                            if (jsonFromInput.ledgerMovements != null)
                            {
                                int affectedRecords = 0;
                                foreach (var jsonMaster in jsonFromInput.ledgerMovements)
                                {
                                    var accountingAccountCode = "";
                                    var costCenterCode = "";
                                    var companyId = "";
                                    if (jsonMaster.CUENTA_CONTABLE != null && jsonMaster.CompanyId != null)
                                    {
                                        accountingAccountCode = jsonMaster.CUENTA_CONTABLE;
                                        costCenterCode = jsonMaster.CENTRO_COSTO;
                                        companyId = jsonMaster.CompanyId;
                                    }
                                    var accountingAccount = _unitOfWork.AccountingAccounts.GetFirstOrDefault(x => x.AccountingAccountCode == accountingAccountCode
                                    && x.CompanyId == companyId);
                                    var costCenter = _unitOfWork.CenterOfCosts.GetFirstOrDefault(x => x.CostCenterCode == costCenterCode
                                    && x.CompanyId == companyId);
                                    LedgerMovement ledgerMovement = new()
                                    {
                                        IdSeat = jsonMaster.ASIENTO,
                                        Consecutive = jsonMaster.CONSECUTIVO,
                                        CostCenterId = costCenter.CostCenterId,
                                        AccountingAccountId = accountingAccount.AccountingAccountId,
                                        Date = jsonMaster.FECHA,
                                        LocalDebit = jsonMaster.DEBITO_LOCAL,
                                        LocalCredit = jsonMaster.CREDITO_LOCAL,
                                        AccountingType = jsonMaster.CONTABILIDAD,
                                        RecordDate = jsonMaster.RecordDate,
                                        CompanyId = companyId
                                    };
                                    if (_unitOfWork.LedgerMovements.AddIfNotExist(ledgerMovement))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                        _unitOfWork.Save();
                                    }
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Movimientos del Mayor /";
                                }
                                updatedRecords = updatedRecords + affectedRecords;
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
                                    if (_unitOfWork.Client.UpdateIfExistAddIfNot(client))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                        _unitOfWork.Save();
                                    }
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Clientes /";
                                }
                                updatedRecords = updatedRecords + affectedRecords;
                            }

                            //INSERT CATEGORY PROVIDERS
                            if (jsonFromInput.providerCategories != null)
                            {
                                int affectedRecords = 0;
                                foreach (var jsonMaster in jsonFromInput.providerCategories)
                                {
                                    ProviderCategory category = new()
                                    {
                                        ProviderCategoryCode = jsonMaster.CATEGORIA_PROVEED,
                                        Description = jsonMaster.DESCRIPCION,
                                        CreateDate = jsonMaster.CreateDate,
                                        CompanyId = jsonMaster.CompanyId
                                    };
                                    if (_unitOfWork.ProviderCategory.UpdateIfExistAddIfNot(category))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                    }
                                    _unitOfWork.Save();
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Categorias de Proveedor /";
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
                                    if (_unitOfWork.Country.UpdateIfExistAddIfNot(country))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                    }
                                    _unitOfWork.Save();
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Paises /";
                                }
                                updatedRecords = updatedRecords + affectedRecords;
                            }

                            //INSERT PROVIDERS
                            if (jsonFromInput.providers != null)
                            {
                                int affectedRecords = 0;
                                foreach (var jsonMaster in jsonFromInput.providers)
                                {
                                    DateTime? lastUpdate = null;
                                    if (jsonMaster.FCH_HORA_ULT_MOD != "")
                                    {
                                        lastUpdate = jsonMaster.FCH_HORA_ULT_MOD;
                                    }
                                    var categoryCode = "";
                                    var companyId = "";
                                    if (jsonMaster.CATEGORIA_PROVEED != null)
                                    {
                                        categoryCode = jsonMaster.CATEGORIA_PROVEED;
                                        companyId = jsonMaster.CompanyId;
                                    }
                                    var categoryProvider = _unitOfWork.ProviderCategory.GetFirstOrDefault(x => x.ProviderCategoryCode == categoryCode
                                    && x.CompanyId == companyId);
                                    int? clientId = null;
                                    if (categoryProvider.Description.Length > 22)
                                    {
                                        var client = _unitOfWork.Client.GetFirstOrDefault(x => x.Name.Contains((categoryProvider.Description).Substring(22))
                                        || x.Alias.Contains((categoryProvider.Description).Substring(22)));
                                        if (client == null)
                                        {
                                            clientId = null;
                                        }
                                        else
                                        {
                                            clientId = client.ClientId;
                                        }
                                    }
                                    Provider provider = new()
                                    {
                                        ProviderCode = jsonMaster.PROVEEDOR,
                                        Name = jsonMaster.NOMBRE,
                                        Alias = jsonMaster.ALIAS,
                                        Occupation = jsonMaster.CARGO,
                                        Address = jsonMaster.DIRECCION,
                                        Email = jsonMaster.E_MAIL,
                                        AdmissionDate = jsonMaster.FECHA_INGRESO,
                                        Phone1 = jsonMaster.TELEFONO1,
                                        Phone2 = jsonMaster.TELEFONO2,
                                        IdCountry = jsonMaster.PAIS,
                                        Id = categoryProvider.Id,
                                        Notes = jsonMaster.NOTAS,
                                        IsActive = jsonMaster.ACTIVO,
                                        DateLastUpdate = lastUpdate,
                                        CreationDate = jsonMaster.CreateDate,
                                        CompanyId = companyId,
                                        ClientId = clientId
                                    };
                                    int? returnedProviderId = _unitOfWork.Provider.UpdateIfExistAddIfNot(provider);
                                    if (returnedProviderId != null)
                                    {
                                        affectedRecords = affectedRecords + 1;
                                        _unitOfWork.Save();
                                        var pEventEntrada = _unitOfWork.ProviderEvent.GetFirstOrDefault(x => x.Name == "Entrada");
                                        var pEventContratoFirmado = _unitOfWork.ProviderEvent.GetFirstOrDefault(x => x.Name == "Contrato Firmado por 1era vez");
                                        var providerEventDateEntrada = _unitOfWork.ProviderEventDate.GetFirstOrDefault(x => x.ProviderId == returnedProviderId
                                        && x.ProviderEventId == pEventEntrada.ProviderEventId);
                                        var providerEventDateContratoFirmado = _unitOfWork.ProviderEventDate.GetFirstOrDefault(x => x.ProviderId == returnedProviderId
                                        && x.ProviderEventId == pEventContratoFirmado.ProviderEventId);
                                        if (providerEventDateEntrada != null && providerEventDateContratoFirmado != null)
                                        {
                                                if (!providerEventDateEntrada.EventDate.Equals(jsonMaster.FECHA_INGRESO))
                                                {
                                                    providerEventDateEntrada.EventDate = jsonMaster.FECHA_INGRESO;
                                                    _unitOfWork.Save();
                                                }
                                                if (!providerEventDateContratoFirmado.EventDate.Equals(jsonMaster.FECHA_INGRESO))
                                                {
                                                    providerEventDateContratoFirmado.EventDate = jsonMaster.FECHA_INGRESO;
                                                    _unitOfWork.Save();
                                                }
                                           
                                        }
                                        else
                                        {
                                            var claimsIdentity = (ClaimsIdentity)User.Identity;
                                            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                                            ProviderEventDate providerEventDate1 = new()
                                            {
                                                ProviderId = (int)returnedProviderId,
                                                EventDate = jsonMaster.FECHA_INGRESO,
                                                ProviderEventId = pEventEntrada.ProviderEventId,
                                                CreatedBy = claim.Value
                                            };
                                            _unitOfWork.ProviderEventDate.Add(providerEventDate1);
                                            _unitOfWork.Save();
                                            ProviderEventDate providerEventDate2 = new()
                                            {
                                                ProviderId = (int)returnedProviderId,
                                                EventDate = jsonMaster.FECHA_INGRESO,
                                                ProviderEventId = pEventContratoFirmado.ProviderEventId,
                                                CreatedBy = claim.Value
                                            };
                                            _unitOfWork.ProviderEventDate.Add(providerEventDate2);
                                            _unitOfWork.Save();
                                        }
                                    }
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Proveedores /";
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
                                    var client = _unitOfWork.Client.GetFirstOrDefault(x => x.ClientCode == clientCode
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
                                    if (_unitOfWork.DocumentCC.UpdateIfExistAddIfNot(document))
                                    {
                                        affectedRecords = affectedRecords + 1;
                                        _unitOfWork.Save();
                                    }
                                }
                                if (affectedRecords > 0)
                                {
                                    updatedSections = updatedSections + "Documentos CC /";
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
                                _unitOfWork.DataUpdateDates.Add(dataUpdateDate);
                                _unitOfWork.Save();
                            }
                            TempData["success"] = updatedRecords + " registros fueron afectados.";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            if (!validateCorrectJsonStructureAccountingAccount(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "La estructura del JSON no es correcta para las Cuentas Contables Contables");
                            }
                            if (!validateCorrectJsonStructureCostCenter(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "La estructura del JSON no es correcta para los Centros de Costo");
                            }
                            if (!validateCorrectJsonStructureLedgerMovement(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "La estructura del JSON no es correcta para los movimientos del Mayor");
                            }
                            if (!validateCorrectJsonStructureClients(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "La estructura del JSON no es correcta para los Clientes");
                            }
                            if (!validateCorrectJsonStructureProviderCategory(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "La estructura del JSON no es correcta para la categoría de Proveedores");
                            }
                            if (!validateCorrectJsonStructureCountry(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "La estructura del JSON no es correcta para los paises");
                            }
                            if (!validateCorrectJsonStructureProvider(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "La estructura del JSON no es correcta para los proveedores");
                            }
                            if (!validateCorrectJsonStructureDocumentsCC(obj.DataToSave))
                            {
                                ModelState.AddModelError("dataToSave", "La estructura del JSON no es correcta para los Documentos CC");
                            }

                            return View("Index");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("dataToSave", "El dato a incluir debe de ser un JSON valido");
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
        public bool validateCorrectJsonStructureProviderCategory(String jsonString)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(jsonString);

                if (json.providerCategories != null)
                {
                    foreach (var result in json.providerCategories)
                    {
                        ProviderCategory category = new()
                        {
                            ProviderCategoryCode = result.CATEGORIA_PROVEED,
                            Description = result.DESCRIPCION,
                            CreateDate = result.CreateDate,
                            CompanyId = result.CompanyId
                        };
                        if (category.ProviderCategoryCode == null || category.Description == null
                            || category.CreateDate.ToString() == null || category.CompanyId == null)
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

        public bool validateCorrectJsonStructureProvider(String jsonString)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(jsonString);

                if (json.providers != null)
                {
                    foreach (var result in json.providers)
                    {
                        DateTime? dateLastUpdate = null;
                        if (result.FCH_HORA_ULT_MOD != "")
                        {
                            dateLastUpdate = result.FCH_HORA_ULT_MOD;
                        }
                        Provider provider = new()
                        {
                            ProviderCode = result.PROVEEDOR,
                            Name = result.NOMBRE,
                            Alias = result.ALIAS,
                            Occupation = result.CARGO,
                            Address = result.DIRECCION,
                            Email = result.E_MAIL,
                            AdmissionDate = result.FECHA_INGRESO,
                            Phone1 = result.TELEFONO1,
                            Phone2 = result.TELEFONO2,
                            IdCountry = result.PAIS,
                            Notes = result.NOTAS,
                            IsActive = result.ACTIVO,
                            DateLastUpdate = dateLastUpdate,
                            CreationDate = result.CreateDate,
                            CompanyId = result.CompanyId
                        };
                        if (provider.ProviderCode == null || provider.Name == null || provider.Occupation == null
                            || provider.AdmissionDate.ToString() == null || provider.IdCountry == null
                            || provider.IsActive == null || provider.CreationDate.ToString() == null || provider.CompanyId == null)
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
    }

}
