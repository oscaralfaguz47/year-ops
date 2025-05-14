using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.DocumentsCCSubtypes;


namespace OceansApp.DataAccess.Repository
{
    public class DocumentCCSubtypeRepository : Repository<DocumentCCSubtype>, IDocumentCCSubtypeRepository
    {
        private ApplicationDbContext _db;
        public DocumentCCSubtypeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<GetDocumentSubtypesListVM>> GetDocumentSubtypesListAsync()
        {
            try
            {
                var results = await (from ds in _db.DOCUMENTS_CC_SUBTYPES
                                    join dt in _db.DOCUMENTS_TYPES on ds.DocumentTypeId equals dt.DocumentTypeId
                                    join cc in _db.COST_CENTER on ds.CostCenterId equals cc.CostCenterId
                                    join ac in _db.ACCOUNTING_ACCOUNT on ds.AccountingAccountId equals ac.AccountingAccountId
                                    join co in _db.COMPANIES on ds.CompanyId equals co.CompanyId
                                     orderby co.CompanyId, ds.Description
                                     select new GetDocumentSubtypesListVM
                                    {
                                        DocumentCCSubtypeId = ds.DocumentCCSybtypeId,
                                        Description = ds.Description,
                                        DocumentType = dt.Description,
                                        Company = co.Name,
                                        CostCenter = cc.Description,
                                        AccountingAccount = ac.Description
                                    }).ToListAsync();

                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<GetDocumentSubtypeVM> GetDocumentSubtypeByIdAsync(int docSubtypeId)
        {
            try
            {
                var result = await (from ds in _db.DOCUMENTS_CC_SUBTYPES
                                    where ds.DocumentCCSybtypeId == docSubtypeId
                                     select new GetDocumentSubtypeVM
                                     {
                                         Description = ds.Description,
                                         DocumentTypeId = ds.DocumentTypeId,
                                         CompanyId = ds.CompanyId,
                                         CostCenterId = ds.CostCenterId,
                                         AccountingAccountId = ds.AccountingAccountId
                                     }).FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<MethodResponse> CreateDocumentSubType(
           CreateUpdateDocumentSubtypeVM docSubtypeData)
        {

            bool existsDocumentSubtypeWithSameDescription = await _db.DOCUMENTS_CC_SUBTYPES.AnyAsync(x => x.Description == docSubtypeData.Description.Trim());

            if (existsDocumentSubtypeWithSameDescription) return MethodResponse
                    .CreateFailureValidationResponse($"The is already a subtype with the description: '{docSubtypeData.Description}'.");

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    DocumentCCSubtype subTypeToCreate = new()
                    {
                        DocumentTypeId = docSubtypeData.DocumentTypeId,
                        Description = docSubtypeData.Description.Trim(),
                        CompanyId = docSubtypeData.CompanyId,
                        CostCenterId = (int)docSubtypeData.CostCenterId,
                        AccountingAccountId = (int)docSubtypeData.AccountingAccountId
                    };
                    var createdSubtype = await _db.DOCUMENTS_CC_SUBTYPES.AddAsync(subTypeToCreate);
                    await _db.SaveChangesAsync();
                    if (createdSubtype.Entity.DocumentCCSybtypeId > 0)
                    {
                        await transaction.CommitAsync();
                        return new MethodResponse
                        {
                            Success = true,
                            Message = $"The Document Subtype was created successfully."
                        };
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"Something went wrong creating the Document Type, please try again." };
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }

        public async Task<MethodResponse> UpdateDocumentSubtype(CreateUpdateDocumentSubtypeVM docSubtypeData)
        {
            bool existsDocumentSubtypeWithSameDescription = await _db.DOCUMENTS_CC_SUBTYPES.AnyAsync(x => x.Description == docSubtypeData.Description.Trim() && x.DocumentCCSybtypeId != docSubtypeData.DocumentCCSubtypeId);

            if (existsDocumentSubtypeWithSameDescription) return MethodResponse
                    .CreateFailureValidationResponse($"The is already a subtype with the description: '{docSubtypeData.Description}'.");

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingDocumentSubtypeToUpdate = await _db.DOCUMENTS_CC_SUBTYPES.FirstOrDefaultAsync(x => x.DocumentCCSybtypeId == docSubtypeData.DocumentCCSubtypeId);
                    if (existingDocumentSubtypeToUpdate == null)
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Not Found", Success = false, Message = $"The Document Type was not found." };
                    }

                    existingDocumentSubtypeToUpdate.CostCenterId = (int)docSubtypeData.CostCenterId;
                    existingDocumentSubtypeToUpdate.AccountingAccountId = (int)docSubtypeData.AccountingAccountId;
                    existingDocumentSubtypeToUpdate.Description = docSubtypeData.Description.Trim();

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new MethodResponse { Success = true, Message = $"The Document Type was updated successfully." };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }

    }
}
