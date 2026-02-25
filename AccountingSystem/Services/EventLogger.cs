using System.Text.Json;
using AccountingSystem.Data;
using AccountingSystem.Models;

namespace AccountingSystem.Services
{
    public interface IEventLogger
    {
        Task LogAsync(string tableName, int recordId, EventAction action, object? before, object? after, string? userId);
    }

    public class EventLogger : IEventLogger
    {
        private readonly ApplicationDbContext _db;

        public EventLogger(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string tableName, int recordId, EventAction action, object? before, object? after, string? userId)
        {
            string? beforeJson = before == null ? null : JsonSerializer.Serialize(before);
            string? afterJson = after == null ? null : JsonSerializer.Serialize(after);

            _db.EventLogs.Add(new EventLog
            {
                TableName = tableName,
                RecordId = recordId,
                Action = (int)action,     // IMPORTANT: store INT
                BeforeJson = beforeJson,
                AfterJson = afterJson,
                UserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
    }
}