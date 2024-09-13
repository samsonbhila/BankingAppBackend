using BankingAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingAppBackend.Dat
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Setting precision and scale for decimal fields to prevent truncation
            modelBuilder.Entity<Account>()
                .Property(a => a.CurrentAccountBalance)
                .HasPrecision(18, 2);  // Set the appropriate precision and scale

            modelBuilder.Entity<Transaction>()
                .Property(t => t.TransactionAmount)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
