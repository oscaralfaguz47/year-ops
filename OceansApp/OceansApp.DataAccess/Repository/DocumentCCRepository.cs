using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.DocumentsCC;
using System.Data;
using System.Text;

namespace OceansApp.DataAccess.Repository
{
    public class DocumentCCRepository : Repository<DocumentCC>, IDocumentCCRepository
    {
        private ApplicationDbContext _db;
        public DocumentCCRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<IEnumerable<SelectVM>> GetDocumentsTypeWhereDocumentsExistAsync()
        {
            var docTypesList = await _db.DOCUMENTS_CC
                .FromSqlRaw(@"SELECT DocumentType FROM DOCUMENTS_CC 
                    GROUP BY DocumentType")
                .Select(c => new SelectVM
                {
                    Value = c.DocumentType,
                    Text = c.DocumentType
                })
                .ToListAsync();

            return docTypesList;
        }


        public async Task<List<DocumentCCGetExpiredDocsVM>> GetAllExpiredDocsWithDaysExpiredFiltersAsync()
        {
            var connection = _db.Database.GetDbConnection();

            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine(@"SELECT 
					DocumentCCId
                    ,DocumentNumber
                    ,DCC.DocumentDate
	                ,DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate) AS ExpirationDate
	                ,ABS(CASE
	                 WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), 
	               	 (DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate))) 
	                 ELSE 0
	                 END) AS NumDaysExpired
	                ,DCC.BalanceAmount
                    ,DCC.DocumentAmount
                    ,C.Name AS ClientName
                    ,(SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) AS NumNotificationsSent
                     FROM DOCUMENTS_CC DCC
                     JOIN CLIENT C ON DCC.ClientId = C.ClientId
					 WHERE DCC.DocumentType = 'FAC' 
					 AND DCC.Canceled = 'N'
					 AND C.ClientCategory NOT LIKE '%CON%'
                     AND C.ClientCode NOT IN('OCELL_C0001')
					 AND DCC.BalanceAmount > 0
                     AND C.AllowSentLatePaymentNotifications = 1
					 AND (
                     (CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END >= 5 
                      AND CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END < 15 
                      AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 0)
                     OR 
                     (CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END >= 15 
                      AND CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END < 30 
                      AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 1)
                     OR 
                     (CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END >= 30 
                      AND CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END < 45 
                      AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 2)
                     OR 
                     (CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END >= 45 
                      AND CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END < 60 
                      AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 3)
	                  OR 
                     (CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END >= 60 
                      AND CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END < 75 
                      AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 4)
	                  OR 
                     (CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END >= 75 
                     AND CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END < 90 
                     AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 5)
	                 OR 
                    (CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END >= 90 
                     AND CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END < 120 
                     AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 6)
	                 OR 
                    (CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END >= 120 
                     AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 7)
                     )
                    ORDER BY NumDaysExpired");

            var results = await connection.QueryAsync<DocumentCCGetExpiredDocsVM>(queryBuilder.ToString());
            var documents = results.ToList();

            return (documents);
        }
        public async Task<List<DocumentCCGetExpiredDocsVM>> GetAllExpiredPendingDocsAsync()
        {
            var connection = _db.Database.GetDbConnection();

            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine(@"SELECT 
					DocumentCCId
                    ,DocumentNumber
                    ,DCC.DocumentDate
	                ,DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate) AS ExpirationDate
	                ,ABS(CASE
	                 WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), 
	               	 (DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate))) 
	                 ELSE 0
	                 END) AS NumDaysExpired
	                ,DCC.BalanceAmount
                    ,DCC.DocumentAmount
                    ,C.Name AS ClientName
                    ,U.Email AS SuccessManagerEmail
                    ,(SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) AS NumNotificationsSent
                     FROM DOCUMENTS_CC DCC
                     JOIN CLIENT C ON DCC.ClientId = C.ClientId
					 LEFT JOIN CONSULTANT_DETAILS CD ON C.SuccessManager = CD.ConsultantId
                     LEFT JOIN Users U ON CD.UserId = U.Id
					 WHERE DCC.DocumentType = 'FAC' 
					 AND DCC.Canceled = 'N'
					 AND C.ClientCategory NOT LIKE '%CON%'
					 AND C.ClientCode NOT IN('OCELL_C0001')
					 AND DCC.BalanceAmount > 0
					 AND (
                     (CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate), SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00')) ELSE 0 END > 1 )
                     )
                    ORDER BY NumDaysExpired");

            var results = await connection.QueryAsync<DocumentCCGetExpiredDocsVM>(queryBuilder.ToString());
            var documents = results.ToList();

            return (documents);
        }
        public async Task<(List<DocumentCCGetAllWithFiltersVM> documentsCC, int totalCount)> GetAllDocumentsCCWithFiltersAsync(DocumentCCPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@DocumentType", filtersAndPagination.Filters.DocumentType, DbType.String);
            parameters.Add("@ClientId", filtersAndPagination.Filters.ClientId, DbType.Int32);
            parameters.Add("@CompanyId", filtersAndPagination.Filters.CompanyId, DbType.String);
            parameters.Add("@StartDate", filtersAndPagination.Filters.StartDate, DbType.Date);
            parameters.Add("@EndDate", filtersAndPagination.Filters.EndDate, DbType.Date);

            parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Ejecutar el procedimiento almacenado
            var results = await connection.QueryAsync<DocumentCCGetAllWithFiltersVM>("SP_DOCUMENTS_CC_GetAllDocumentsCCWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");
            var documentsCC = results.ToList();

            return (documentsCC, totalCount);
        }

        public async Task<List<DocumentCCGetNotificationsHistoryVM>> GetNotificationsHistoryByDocumentIdAsync(int documentId)
        {
            var connection = _db.Database.GetDbConnection();

            var queryBuilder = new StringBuilder();
            var parameters = new DynamicParameters();

            queryBuilder.AppendLine(@"SELECT 
                                    N.NotificationId
                                    ,N.SentDate 
                                    ,U.Name + ' ' + U.LastName AS SentByUser
                                    FROM DOCUMENTS_CC_NOTIFICATIONS DN
                                    JOIN NOTIFICATIONS N ON DN.NotificationId = N.NotificationId
                                    JOIN Users U ON N.SentByUser = U.Id
                                    WHERE DN.DocumentCCId = @documentId
                                    ORDER BY N.SentDate DESC");

            parameters.Add("@documentId", documentId, DbType.String);

            var results = await connection.QueryAsync<DocumentCCGetNotificationsHistoryVM>(queryBuilder.ToString(), parameters);
            var documents = results.ToList();

            return (documents);
        }

        public void Update(DocumentCC obj)
        {
            _db.DOCUMENTS_CC.Update(obj);
        }

        public async Task<bool> UpdateIfExistAddIfNot(DocumentCC obj)
        {
            var existingDoc = await GetFirstOrDefaultAsync(u => u.DocumentNumber == obj.DocumentNumber && u.DocumentType == obj.DocumentType && u.CompanyId == obj.CompanyId);
            if (existingDoc == null)
            {
                _db.DOCUMENTS_CC.Add(obj);
                _db.SaveChanges();
                return true;
            }
            else
            {
                if (existingDoc.DateLastUpdate != obj.DateLastUpdate
                    || existingDoc.BalanceAmount != obj.BalanceAmount
                    || existingDoc.Canceled != obj.Canceled)
                {
                    existingDoc.DocumentNumber = obj.DocumentNumber;
                    existingDoc.DocumentType = obj.DocumentType;
                    existingDoc.ApplicationDescription = obj.ApplicationDescription;
                    existingDoc.DocumentDate = obj.DocumentDate;
                    existingDoc.DocumentAmount = obj.DocumentAmount;
                    existingDoc.BalanceAmount = obj.BalanceAmount;
                    existingDoc.Canceled = obj.Canceled;
                    existingDoc.IdSeat = obj.IdSeat;
                    existingDoc.DateLastUpdate = obj.DateLastUpdate;
                    existingDoc.CreationDate = obj.CreationDate;
                    existingDoc.CompanyId = obj.CompanyId;
                    return true;
                }
                return false;
            }
        }

        public async Task<GetSubtypesListAndDocTypeConsecutiveNumberVM> GetDocumentSubtypesListAndDocTypeConsecutiveNumberAsync(string docTypeId, int clientConsultantId,
            bool isClient, bool isCredit)
        {
            try
            {
                string companyId = "";

                if (isClient)
                {
                    var client = await _db.CLIENT.FirstOrDefaultAsync(x => x.ClientId == clientConsultantId);
                    companyId = client.CompanyId;
                }
                else
                {
                    var consultant = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.ConsultantId == clientConsultantId);
                    companyId = consultant.CompanyId;
                }


                var results = await (from ds in _db.DOCUMENTS_CC_SUBTYPES
                                     where ds.CompanyId == companyId && ds.DocumentTypeId == docTypeId
                                     orderby ds.Description
                                     select new SelectVM
                                     {
                                         Value = ds.DocumentCCSybtypeId.ToString(),
                                         Text = ds.Description
                                     }).ToListAsync();

                var docConsecutiveNumber = 0;

                if (isCredit)
                {
                    var consecutiveNumberData = await _db.GLOBAL_CONSECUTIVES.FirstOrDefaultAsync(x => x.Name == docTypeId && x.CompanyId == companyId);
                    docConsecutiveNumber = consecutiveNumberData.ConsecutiveNumber;
                    docConsecutiveNumber++;
                }
              

                GetSubtypesListAndDocTypeConsecutiveNumberVM modelToReturn = new()
                {
                    SubtypesList = results,
                    DocConsecutiveNumber = docConsecutiveNumber
                };

                return modelToReturn;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
