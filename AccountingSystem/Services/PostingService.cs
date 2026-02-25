using System.Text.Json;
using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Services
{
    public interface IPostingService
    {
        Task<(bool ok, string message)> PostAsync(int journalEntryId, string? postedByUserId);
    }

    public class PostingService : IPostingService
    {
        private readonly ApplicationDbContext _db;

        public PostingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<(bool ok, string message)> PostAsync(int journalEntryId, string? postedByUserId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var entry = await _db.JournalEntries
                .Include(e => e.Lines)
                    .ThenInclude(l => l.ChartAccount)
                .FirstOrDefaultAsync(e => e.JournalEntryId == journalEntryId);

            if (entry == null) return (false, "Journal entry not found.");
            if (entry.Status != JournalStatus.Approved) return (false, "Only approved journal entries can be posted.");
            if (entry.Lines.Count == 0) return (false, "Journal entry has no lines.");

            // Before snapshot for journal entry
            var beforeJournal = SnapshotJournal(entry);

            var totalD = entry.Lines.Sum(l => l.Debit);
            var totalC = entry.Lines.Sum(l => l.Credit);
            if (totalD != totalC) return (false, "Cannot post: debits and credits do not balance.");

            var affectedIds = entry.Lines.Select(l => l.ChartAccountId).Distinct().ToList();
            var accounts = await _db.ChartAccounts
                .Where(a => affectedIds.Contains(a.ChartAccountId))
                .ToDictionaryAsync(a => a.ChartAccountId);

            foreach (var line in entry.Lines)
            {
                if (!accounts.TryGetValue(line.ChartAccountId, out var acct))
                    return (false, "Cannot post: referenced account not found.");

                // Before snapshot for this account
                var beforeAcct = SnapshotAccount(acct);

                var delta = AccountingMath.SignedImpact(acct.NormalSide, line.Debit, line.Credit);
                var newBalance = acct.Balance + delta;

                _db.LedgerEntries.Add(new LedgerEntry
                {
                    ChartAccountId = acct.ChartAccountId,
                    EntryDate = entry.EntryDate,
                    JournalEntryId = entry.JournalEntryId,
                    Description = entry.Description,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    BalanceAfter = newBalance,
                    PostedAtUtc = DateTime.UtcNow
                });

                acct.Balance = newBalance;
                acct.Debit += line.Debit;
                acct.Credit += line.Credit;

                // After snapshot for this account
                var afterAcct = SnapshotAccount(acct);

                AddEvent(
                    table: "ChartAccounts",
                    recordId: acct.ChartAccountId,
                    action: EventAction.Update,
                    beforeObj: beforeAcct,
                    afterObj: afterAcct,
                    userId: postedByUserId
                );
            }

            entry.Status = JournalStatus.Posted;
            entry.PostedByUserId = postedByUserId;
            entry.PostedAtUtc = DateTime.UtcNow;

            var afterJournal = SnapshotJournal(entry);

            AddEvent(
                table: "JournalEntries",
                recordId: entry.JournalEntryId,
                action: EventAction.Post,
                beforeObj: beforeJournal,
                afterObj: afterJournal,
                userId: postedByUserId
            );

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return (true, "Posted successfully.");
        }

        // ----------------- helpers -----------------
        private void AddEvent(string table, int recordId, EventAction action, object? beforeObj, object? afterObj, string? userId)
        {
            _db.EventLogs.Add(new EventLog
            {
                TableName = table,
                RecordId = recordId,
                Action = (int)action,
                BeforeJson = beforeObj == null ? null : JsonSerializer.Serialize(beforeObj),
                AfterJson = afterObj == null ? null : JsonSerializer.Serialize(afterObj),
                UserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        private static object SnapshotJournal(JournalEntry e) => new
        {
            e.JournalEntryId,
            e.EntryDate,
            e.Status,
            e.ManagerComment,
            e.CreatedByUserId,
            e.CreatedAtUtc,
            e.ApprovedByUserId,
            e.ApprovedAtUtc,
            e.PostedByUserId,
            e.PostedAtUtc
        };

        private static object SnapshotAccount(ChartAccount a) => new
        {
            a.ChartAccountId,
            a.AccountNumber,
            a.AccountName,
            a.NormalSide,
            a.Category,
            a.Subcategory,
            a.Debit,
            a.Credit,
            a.Balance,
            a.IsActive
        };
    }
}