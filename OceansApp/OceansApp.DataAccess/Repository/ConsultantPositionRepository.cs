using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPositions;
using System.Data;


namespace OceansApp.DataAccess.Repository
{
    public class ConsultantPositionRepository : Repository<ConsultantPosition>, IConsultantPositionRepository
    {
        private ApplicationDbContext _db;
        public ConsultantPositionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<(List<ConsultantPositionsGetAllWithFiltersVM> positions, int totalCount)> GetAllConsultantPositionsWithFiltersAsync(ConsultantPositionsPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@MovementTypeId", filtersAndPagination.Filters.MovementTypeId, DbType.Int32);
            parameters.Add("@CostCenterId", filtersAndPagination.Filters.CostCenterId, DbType.Int32);
            parameters.Add("@AccountingAccountId", filtersAndPagination.Filters.AccountingAccountId, DbType.Int32);

            parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<ConsultantPositionsGetAllWithFiltersVM>("SP_CONSULTANT_POSITIONS_GetAllPositionsAccountingConfigurationWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");
            var positions = results.ToList();

            return (positions, totalCount);
        }

        public async Task<List<GetConsultantPositionConfigurationsVM>> GetCompanyMovementTypesByPositionIdAsync(int? positionId)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PositionId", positionId, DbType.Int32);

            var results = await connection.QueryAsync<GetConsultantPositionConfigurationsVM>("SP_CONSULTANT_POSITIONS_GetCompanyMovementTypeByPositionId", parameters, commandType: CommandType.StoredProcedure);

            return results.ToList();
        }

        public async Task<MethodResponse> CreatePositionAsync(CreateUpdateConsultantPositionVM positionConfigData)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPosition = await _db.CONSULTANT_POSITIONS.FirstOrDefaultAsync(x => x.Name.Trim() == positionConfigData.PositionName.Trim());
                    if (existingPosition != null)
                    {
                        return new MethodResponse { MessageType = "Validation Error", Success = false, Message = $"There is already a position with the name: {positionConfigData.PositionName}" };
                    }

                    ConsultantPosition positionToCreate = new ConsultantPosition()
                    {
                        Name = positionConfigData.PositionName.Trim(),
                        IsAdministrative = (bool)positionConfigData.IsAdministrative
                    };
                    await _db.CONSULTANT_POSITIONS.AddAsync(positionToCreate);
                    await _db.SaveChangesAsync();

                    foreach (var positionConfig in positionConfigData.PositionConfiguration)
                    {
                        ConsultantPositionAccountingConfiguration accountingConfigToCreate = new ConsultantPositionAccountingConfiguration()
                        {
                            CompanyId = positionConfig.CompanyId,
                            CostCenterId = (int)positionConfig.CostCenterId,
                            AccountingAccountId = (int)positionConfig.AccountingAccountId,
                            MovementTypeId = positionConfig.MovementTypeId,
                            PositionId = positionToCreate.ConsultantPositionId
                        };
                        await _db.CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION.AddAsync(accountingConfigToCreate);
                    }
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new MethodResponse
                    {
                        Success = true,
                        Message = $"The Position was created successfully."
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }

        public async Task<MethodResponse> UpdatePositionAsync(CreateUpdateConsultantPositionVM positionConfigData)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPositionGetFromName = await _db.CONSULTANT_POSITIONS.FirstOrDefaultAsync(x => x.Name.Trim() == positionConfigData.PositionName.Trim());
                    var existingPositionGetFromId = await _db.CONSULTANT_POSITIONS.FirstOrDefaultAsync(x => x.ConsultantPositionId == positionConfigData.PositionId);
                    if (existingPositionGetFromId == null)
                    {
                        return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The position is no longer in the database" };
                    }

                    if (existingPositionGetFromName != null && (existingPositionGetFromName.ConsultantPositionId
                        != existingPositionGetFromId.ConsultantPositionId))
                    {
                        return new MethodResponse { MessageType = "Validation Error", Success = false, Message = $"There is already a position with the name: {positionConfigData.PositionName}" };
                    }

                    existingPositionGetFromId.Name = positionConfigData.PositionName.Trim();
                    existingPositionGetFromId.IsAdministrative = (bool)positionConfigData.IsAdministrative;

                    foreach (var positionConfig in positionConfigData.PositionConfiguration)
                    {
                        if (positionConfig.Id != null)
                        {
                            var existingPositionAccountingConfig = await _db.CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION
                                .FirstOrDefaultAsync(x => x.Id == positionConfig.Id);
                            if (existingPositionAccountingConfig == null)
                            {
                                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The configuration is no longer in the database" };
                            }
                            existingPositionAccountingConfig.CostCenterId = (int)positionConfig.CostCenterId;
                            existingPositionAccountingConfig.AccountingAccountId = (int)positionConfig.AccountingAccountId;
                        }
                        else
                        {
                            ConsultantPositionAccountingConfiguration accountingConfigToCreate = new ConsultantPositionAccountingConfiguration()
                            {
                                CompanyId = positionConfig.CompanyId,
                                CostCenterId = (int)positionConfig.CostCenterId,
                                AccountingAccountId = (int)positionConfig.AccountingAccountId,
                                MovementTypeId = positionConfig.MovementTypeId,
                                PositionId = (int)positionConfigData.PositionId
                            };
                            await _db.CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION.AddAsync(accountingConfigToCreate);
                        }
                    }
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new MethodResponse
                    {
                        Success = true,
                        Message = $"The Position was updated successfully."
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }
        public async Task<List<GetDataForSelectVM>> GetPositionsByIsAdministrative(bool isAdministrative)
        {
            IEnumerable<ConsultantPosition> positionsListFromDb = await _db.CONSULTANT_POSITIONS.Where(x => x.IsAdministrative == isAdministrative).ToListAsync();
            List<GetDataForSelectVM> positionsToReturn = new();
            foreach (var position in positionsListFromDb)
            {
                positionsToReturn.Add(new GetDataForSelectVM
                {
                    Value = position.ConsultantPositionId,
                    Text = position.Name
                });
            }
            return positionsToReturn;
        }

        public async Task<List<GetConsultantPostionsForSelectVM>> GetPositionsByConsultantIdAsync(int consultantId)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ConsultantId", consultantId, DbType.Int32);

            var results = await connection.QueryAsync<GetConsultantPostionsForSelectVM>("SP_CONSULTANT_POSITIONS_GetPositionsByConsultantId", parameters, commandType: CommandType.StoredProcedure);
            var positions = results.ToList();

            return positions;
        }
    }
}
