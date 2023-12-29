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
        IEnumerable<SelectVM> IDocumentCCRepository.GetDocumentsTypeWhereDocumentsExist()
        {
            IEnumerable<SelectVM> docTypesList = _db.DOCUMENTS_CC
                .FromSqlRaw(@"SELECT DocumentType FROM DOCUMENTS_CC 
                            GROUP BY DocumentType
        ")
                .Select(c => new SelectVM
                {
                    Value = c.DocumentType,
                    Name = c.DocumentType
                })
                .ToList();
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
					 AND DCC.BalanceAmount > 0
					 AND (
                     (ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) >= 5 
                      AND ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) < 15 
                      AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 0)
                     OR 
                     (ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) >= 15 
                      AND ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) < 30 
                      AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 1)
                     OR 
                     (ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) >= 30 
                      AND ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) < 45 
                      AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 2)
                     OR 
                     (ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) >= 45 
                      AND ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) < 60 
                      AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 3)
	                  OR 
                     (ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) >= 60 
                      AND ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) < 75 
                      AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 4)
	                  OR 
                     (ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) >= 75 
                     AND ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) < 90 
                     AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 5)
	                 OR 
                    (ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) >= 90 
                     AND ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) < 120 
                     AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 6)
	                 OR 
                    (ABS(CASE WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate)) ELSE 0 END) >= 120 
                     AND (SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) = 7)
                     )
                    ORDER BY NumDaysExpired");

            var results = await connection.QueryAsync<DocumentCCGetExpiredDocsVM>(queryBuilder.ToString());
            var documents = results.ToList();

            return (documents);
        }
        public async Task<(List<DocumentCCGetAllWithFiltersVM> documentsCC, int totalCount)> GetAllDocumentsCCWithFiltersAsync(DocumentCCGetAllForListVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var queryBuilder = new StringBuilder();
            var parameters = new DynamicParameters();

            queryBuilder.AppendLine(@"SELECT DocumentCCId
                    ,DocumentNumber
                    ,DCC.DocumentType
                    ,DCC.ApplicationDescription
                    ,DCC.DocumentDate
	                ,DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate) AS ExpirationDate
	                ,CASE
	                 WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), 
	               	 (DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate))) 
	                 ELSE 0
	                 END AS NumDaysToExpire
	                ,DCC.BalanceAmount
                    ,DCC.DocumentAmount
                    ,DCC.Canceled
                    ,C.Name AS ClientName
                    ,DCC.CompanyId
                    ,C.ClientCategory
                    ,(SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) AS NumNotificationsSent
                     FROM DOCUMENTS_CC DCC
                     JOIN CLIENT C ON DCC.ClientId = C.ClientId
                     WHERE ((@SearchText IS NULL OR LOWER(DCC.DocumentNumber) LIKE '%' + LOWER(@SearchText) + '%')
                     OR (@SearchText IS NULL OR LOWER(DCC.ApplicationDescription) LIKE '%' + LOWER(@SearchText) + '%'))
                     AND (@ClientId IS NULL OR DCC.ClientId = @ClientId)
                     AND (@CompanyId IS NULL OR DCC.CompanyId = @CompanyId)
                     AND (@DocumentType IS NULL OR DCC.DocumentType = @DocumentType)
                     AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (DCC.DocumentDate >= @StartDate AND DCC.DocumentDate <= @EndDate))");

            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@DocumentType", filtersAndPagination.Filters.DocumentType, DbType.String);
            parameters.Add("@ClientId", filtersAndPagination.Filters.ClientId, DbType.Int32);
            parameters.Add("@CompanyId", filtersAndPagination.Filters.CompanyId, DbType.String);
            parameters.Add("@StartDate", filtersAndPagination.Filters.StartDate, DbType.String);
            parameters.Add("@EndDate", filtersAndPagination.Filters.EndDate, DbType.String);

            var countQuery = "SELECT COUNT(*) FROM (" + queryBuilder.ToString() + ") AS TotalCountQuery;";
            var totalCount = await connection.ExecuteScalarAsync<int>(countQuery, parameters);

            // Aplica pagination to the query
            queryBuilder.AppendLine("ORDER BY DCC.DocumentType, NumDaysToExpire ASC");
            queryBuilder.AppendLine("OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;");

            parameters.Add("@Skip", (filtersAndPagination.Pagination.PageIndex - 1) * filtersAndPagination.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.Pagination.PageSize, DbType.Int32);

            var results = await connection.QueryAsync<DocumentCCGetAllWithFiltersVM>(queryBuilder.ToString(), parameters);
            var documents = results.ToList();

            return (documents, totalCount);
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

        public bool UpdateIfExistAddIfNot(DocumentCC obj)
        {
            var existingDoc = GetFirstOrDefault(u => u.DocumentNumber == obj.DocumentNumber && u.DocumentType == obj.DocumentType && u.CompanyId == obj.CompanyId);
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

    }
}
