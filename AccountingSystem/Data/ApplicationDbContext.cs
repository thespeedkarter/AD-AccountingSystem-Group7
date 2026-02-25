using AccountingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AccessRequest> AccessRequests { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
        public DbSet<UserSecurity> UserSecurities { get; set; }
        public DbSet<ChartAccount> ChartAccounts { get; set; }

        public DbSet<EventLog> EventLogs { get; set; }  // ADD THIS

        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalLine> JournalLines { get; set; }
        public DbSet<JournalAttachment> JournalAttachments { get; set; }
        public DbSet<LedgerEntry> LedgerEntries { get; set; }

        public DbSet<AppErrorMessage> AppErrorMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PasswordHistory>()
                .HasIndex(p => new { p.UserId, p.CreatedAt });

            builder.Entity<UserSecurity>()
                .HasKey(x => x.UserId);

            builder.Entity<ChartAccount>()
                .HasIndex(a => a.AccountNumber)
                .IsUnique();

            builder.Entity<ChartAccount>()
                .HasIndex(a => a.AccountName)
                .IsUnique();

            builder.Entity<EventLog>()
                .Property(x => x.TableName)
                .HasMaxLength(100);

            builder.Entity<EventLog>()
                .Property(x => x.UserId)
                .HasMaxLength(450);
        }
    }
}