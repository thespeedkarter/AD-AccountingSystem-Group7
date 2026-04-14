using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Services
{
    public class TrialBalanceRow
    {
        public int ChartAccountId { get; set; }
        public int AccountNumber { get; set; }
        public string AccountName { get; set; } = "";
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public class StatementRow
    {
        public int ChartAccountId { get; set; }
        public int AccountNumber { get; set; }
        public string AccountName { get; set; } = "";
        public string? Subcategory { get; set; }
        public decimal Amount { get; set; }
    }

    public interface IFinancialReportService
    {
        Task<List<TrialBalanceRow>> GetTrialBalanceAsync(DateTime? from, DateTime? to);
        Task<List<StatementRow>> GetIncomeStatementAsync(DateTime? from, DateTime? to);
        Task<List<StatementRow>> GetBalanceSheetAsync(DateTime? asOfDate);
        Task<List<StatementRow>> GetRetainedEarningsAsync(DateTime? from, DateTime? to);
    }

    public class FinancialReportService : IFinancialReportService
    {
        private readonly ApplicationDbContext _db;

        public FinancialReportService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<TrialBalanceRow>> GetTrialBalanceAsync(DateTime? from, DateTime? to)
        {
            var rows = await _db.ChartAccounts
                .AsNoTracking()
                .Where(a => a.IsActive)
                .OrderBy(a => a.OrderCode)
                .ThenBy(a => a.AccountNumber)
                .Select(a => new TrialBalanceRow
                {
                    ChartAccountId = a.ChartAccountId,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Debit = a.NormalSide == NormalSide.Debit
                        ? a.Balance
                        : 0m,
                    Credit = a.NormalSide == NormalSide.Credit
                        ? a.Balance
                        : 0m
                })
                .ToListAsync();

            return rows;
        }

        public async Task<List<StatementRow>> GetIncomeStatementAsync(DateTime? from, DateTime? to)
        {
            var rows = await _db.ChartAccounts
                .AsNoTracking()
                .Where(a => a.IsActive && a.Statement == "IS")
                .OrderBy(a => a.OrderCode)
                .ThenBy(a => a.AccountNumber)
                .Select(a => new StatementRow
                {
                    ChartAccountId = a.ChartAccountId,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Subcategory = a.Subcategory,
                    Amount = a.Category == AccountCategory.Revenue
                        ? a.Balance
                        : a.Category == AccountCategory.Expense
                            ? -a.Balance
                            : a.Balance
                })
                .ToListAsync();

            return rows;
        }

        public async Task<List<StatementRow>> GetBalanceSheetAsync(DateTime? asOfDate)
        {
            var rows = await _db.ChartAccounts
                .AsNoTracking()
                .Where(a => a.IsActive && a.Statement == "BS")
                .OrderBy(a => a.OrderCode)
                .ThenBy(a => a.AccountNumber)
                .Select(a => new StatementRow
                {
                    ChartAccountId = a.ChartAccountId,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Subcategory = a.Subcategory,
                    Amount = a.Balance
                })
                .ToListAsync();

            return rows;
        }

        public async Task<List<StatementRow>> GetRetainedEarningsAsync(DateTime? from, DateTime? to)
        {
            var rows = await _db.ChartAccounts
                .AsNoTracking()
                .Where(a => a.IsActive && a.Statement == "RE")
                .OrderBy(a => a.OrderCode)
                .ThenBy(a => a.AccountNumber)
                .Select(a => new StatementRow
                {
                    ChartAccountId = a.ChartAccountId,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    Subcategory = a.Subcategory,
                    Amount = a.Balance
                })
                .ToListAsync();

            return rows;
        }
    }
}