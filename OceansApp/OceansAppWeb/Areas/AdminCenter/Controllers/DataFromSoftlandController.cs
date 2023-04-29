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
    [Authorize(Roles = SD.Role_User_Master)]
    public class DataFromSoftlandController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public DataFromSoftlandController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
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
                            && validateCorrectJsonStructureClients(obj.DataToSave))
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
                                        IdAccountingAccount = jsonMaster.CUENTA_CONTABLE,
                                        Description = jsonMaster.DESCRIPCION,
                                        AccountingAccountType = jsonMaster.TIPO,
                                        DetailedType = jsonMaster.TIPO_DETALLADO,
                                        Balance = jsonMaster.SALDO_NORMAL,
                                        AcceptData = jsonMaster.ACEPTA_DATOS,
                                        UseCostCenter = jsonMaster.USA_CENTRO_COSTO,
                                        UseThird = jsonMaster.MANEJA_TERCERO,
                                        DateLastUpdate = jsonMaster.FCH_HORA_ULT_MOD,
                                        DateHour = jsonMaster.FECHA_HORA
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
                                        IdCostCenter = jsonMaster.CENTRO_COSTO,
                                        Description = jsonMaster.DESCRIPCION,
                                        AcceptData = jsonMaster.ACEPTA_DATOS,
                                        CreateDate = jsonMaster.CreateDate
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
                                    LedgerMovement ledgerMovement = new()
                                    {
                                        IdSeat = jsonMaster.ASIENTO,
                                        Consecutive = jsonMaster.CONSECUTIVO,
                                        IdCostCenter = jsonMaster.CENTRO_COSTO,
                                        IdAccountingAccount = jsonMaster.CUENTA_CONTABLE,
                                        Date = jsonMaster.FECHA,
                                        LocalDebit = jsonMaster.DEBITO_LOCAL,
                                        LocalCredit = jsonMaster.CREDITO_LOCAL,
                                        AccountingType = jsonMaster.CONTABILIDAD,
                                        RecordDate = jsonMaster.RecordDate
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
                                        IdClient = jsonMaster.CLIENTE,
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
                                        CreationDate = jsonMaster.CreateDate
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
                            return View("Index");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("dataToSave", "El dato a incluir debe de ser un JSON valido");
                        return View("Index");
                    }
                }
                TempData["success"] = updatedRecords + " registros fueron afectados.";
                return RedirectToAction("Index");
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
                            IdAccountingAccount = result.CUENTA_CONTABLE,
                            Description = result.DESCRIPCION,
                            AccountingAccountType = result.TIPO,
                            DetailedType = result.TIPO_DETALLADO,
                            Balance = result.SALDO_NORMAL,
                            AcceptData = result.ACEPTA_DATOS,
                            UseCostCenter = result.USA_CENTRO_COSTO,
                            UseThird = result.MANEJA_TERCERO,
                            DateLastUpdate = result.FCH_HORA_ULT_MOD,
                            DateHour = result.FECHA_HORA
                        };
                        if (accountingAccount.IdAccountingAccount == null || accountingAccount.Description == null || accountingAccount.AccountingAccountType == null
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
                            IdCostCenter = result.CENTRO_COSTO,
                            Description = result.DESCRIPCION,
                            AcceptData = result.ACEPTA_DATOS,
                            CreateDate = result.CreateDate
                        };
                        if (costCenter.IdCostCenter == null || costCenter.Description == null || costCenter.AcceptData == null
                            || costCenter.CreateDate.ToString() == null)
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
                            IdCostCenter = result.CENTRO_COSTO,
                            IdAccountingAccount = result.CUENTA_CONTABLE,
                            Date = result.FECHA,
                            LocalDebit = result.DEBITO_LOCAL,
                            LocalCredit = result.CREDITO_LOCAL,
                            AccountingType = result.CONTABILIDAD,
                            RecordDate = result.RecordDate
                        };
                        if (ledgerMovement.IdSeat == null || ledgerMovement.Consecutive.ToString() == null || ledgerMovement.IdCostCenter == null
                            || ledgerMovement.IdAccountingAccount == null || ledgerMovement.Date.ToString() == null
                            || ledgerMovement.LocalDebit.ToString() == null || ledgerMovement.LocalCredit.ToString() == null
                            || ledgerMovement.AccountingType == null || ledgerMovement.RecordDate.ToString() == null)
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
                            IdClient = result.CLIENTE,
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
                            CreationDate = result.CreateDate
                        };
                        if (client.IdClient == null || client.Name == null || client.AdmissionDate.ToString() == null
                            || client.PaymentCondition == null || client.Discount.ToString() == null
                            || client.IsActive == null || client.ClientCategory == null
                            || client.CreationDate.ToString() == null)
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
