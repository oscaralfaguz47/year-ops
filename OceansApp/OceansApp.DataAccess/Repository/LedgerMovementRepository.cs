using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;

namespace OceansApp.DataAccess.Repository
{
    public class LedgerMovementRepository : Repository<LedgerMovement>, ILedgerMovementRepository
    {
        private ApplicationDbContext _db;
        public LedgerMovementRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<IEnumerable<AccountingAccountWithBalanceVM>> GetAccountingAccountsWithBalance(string accountingAccountIdBegin,
        DateTime fechaInicial, DateTime fechaFinal, int ignoreAccountingAccounts, string balance)
        {
            var query = @"
SELECT
LM.IdAccountingAccount
,AA.Description
,LM.IdCostCenter
," + (balance == "D" ? "SUM(LM.LocalDebit) - SUM(LM.LocalCredit)" : "SUM(LM.LocalCredit) - SUM(LM.LocalDebit)") + @" AS TotalAmount
FROM LEDGER_MOVEMENT LM
JOIN ACCOUNTING_ACCOUNT AA ON LM.IdAccountingAccount = AA.IdAccountingAccount
WHERE LM.IdAccountingAccount LIKE CONCAT(@accountingAccountIdBegin,'%')
AND (LM.Date >= @fechaInicial AND LM.Date <= @fechaFinal)
" + (ignoreAccountingAccounts == 1 ? "AND LM.IdAccountingAccount NOT IN (SELECT IdAccountingAccount FROM CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE)" : "") + @"
GROUP BY LM.IdAccountingAccount, AA.Description, LM.IdCostCenter 
ORDER BY LM.IdAccountingAccount";

            List<AccountingAccountWithBalanceVM> accountingAccountsList = new List<AccountingAccountWithBalanceVM>();

            using (var command = _db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.Parameters.Add(new SqlParameter("@accountingAccountIdBegin", accountingAccountIdBegin));
                command.Parameters.Add(new SqlParameter("@fechaInicial", fechaInicial));
                command.Parameters.Add(new SqlParameter("@fechaFinal", fechaFinal));

                await _db.Database.OpenConnectionAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var accountingAccount = new AccountingAccountWithBalanceVM
                        {
                            AccountingAccountCode = reader["IdAccountingAccount"].ToString(),
                            AccountingAccountName = reader["Description"].ToString(),
                            CostCenterId = (int)reader["CostCenterId"],
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"])
                        };

                        accountingAccountsList.Add(accountingAccount);
                    }
                }
            }

            return accountingAccountsList;
        }


        public async Task<IEnumerable<AccountingAccountWithBalanceVM>> GetAccountingAccountsReturnsAndDiscountsWithBalance(
            DateTime fechaInicial, DateTime fechaFinal, int ignoreAccountingAccounts)
        {
            var query = @"
        SELECT
        LM.IdAccountingAccount
        ,AA.Description
        ,SUM(LM.LocalDebit) - SUM(LM.LocalCredit) AS TotalAmount
        FROM LEDGER_MOVEMENT LM
        JOIN ACCOUNTING_ACCOUNT AA ON LM.IdAccountingAccount = AA.IdAccountingAccount
        WHERE LM.IdAccountingAccount LIKE '4-02-01%' OR LM.IdAccountingAccount LIKE '4-03-01%'
        AND (LM.Date >= @fechaInicial AND LM.Date <= @fechaFinal)
        " + (ignoreAccountingAccounts == 1 ? "AND LM.IdAccountingAccount NOT IN (SELECT IdAccountingAccount FROM CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE)" : "") + @"
        GROUP BY LM.IdAccountingAccount, AA.Description
        ORDER BY LM.IdAccountingAccount";

            List<AccountingAccountWithBalanceVM> accountingAccountsList = new List<AccountingAccountWithBalanceVM>();

            using (var command = _db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.Parameters.Add(new SqlParameter("@fechaInicial", fechaInicial));
                command.Parameters.Add(new SqlParameter("@fechaFinal", fechaFinal));

                await _db.Database.OpenConnectionAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var accountingAccount = new AccountingAccountWithBalanceVM
                        {
                            AccountingAccountCode = reader["IdAccountingAccount"].ToString(),
                            AccountingAccountName = reader["Description"].ToString(),
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"])
                        };

                        accountingAccountsList.Add(accountingAccount);
                    }
                }
            }

            return accountingAccountsList;
        }

        public void Update(LedgerMovement obj)
        {
            _db.LEDGER_MOVEMENT.Update(obj);
        }
        public bool AddIfNotExist(LedgerMovement obj)
        {
            var existingLedgerMovement = GetFirstOrDefault(u => u.IdSeat == obj.IdSeat && u.CostCenterId == obj.CostCenterId &&
            u.AccountingAccountId == obj.AccountingAccountId && u.LocalDebit == obj.LocalDebit &&
            u.LocalCredit == obj.LocalCredit && u.Consecutive == obj.Consecutive && u.CompanyId == obj.CompanyId);

            if (existingLedgerMovement == null)
            {
                _db.LEDGER_MOVEMENT.Add(obj);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
