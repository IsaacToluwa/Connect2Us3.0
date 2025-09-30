using System.Data.Entity;

namespace book2us.Models
{
    public class Book2UsContext : DbContext
    {
        public Book2UsContext() : base("name=Book2UsContext")
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<PrintingRequest> PrintingRequests { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<CardDetails> CardDetails { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure WithdrawalRequest relationships to prevent cascade delete cycles
            modelBuilder.Entity<WithdrawalRequest>()
                .HasRequired(wr => wr.User)
                .WithMany()
                .HasForeignKey(wr => wr.UserId)
                .WillCascadeOnDelete(false); // Disable cascade delete for User -> WithdrawalRequest

            modelBuilder.Entity<WithdrawalRequest>()
                .HasRequired(wr => wr.BankAccount)
                .WithMany()
                .HasForeignKey(wr => wr.BankAccountId)
                .WillCascadeOnDelete(false); // Disable cascade delete for BankAccount -> WithdrawalRequest

            // Also configure BankAccount relationship to prevent cascade delete issues
            modelBuilder.Entity<BankAccount>()
                .HasRequired(ba => ba.User)
                .WithMany()
                .HasForeignKey(ba => ba.UserId)
                .WillCascadeOnDelete(true); // Keep cascade delete for User -> BankAccount (this should be safe)
        }
    }
}