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

            // Configure Transaction relationships
            modelBuilder.Entity<Transaction>()
                .HasRequired(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .WillCascadeOnDelete(false); // Disable cascade delete to prevent cycles

            modelBuilder.Entity<Transaction>()
                .HasOptional(t => t.BankAccount)
                .WithMany()
                .HasForeignKey(t => t.BankAccountId)
                .WillCascadeOnDelete(false); // Disable cascade delete

            modelBuilder.Entity<Transaction>()
                .HasOptional(t => t.Card)
                .WithMany()
                .HasForeignKey(t => t.CardId)
                .WillCascadeOnDelete(false); // Disable cascade delete

            modelBuilder.Entity<Transaction>()
                .HasOptional(t => t.RelatedTransaction)
                .WithMany()
                .HasForeignKey(t => t.RelatedTransactionId)
                .WillCascadeOnDelete(false); // Disable cascade delete to prevent self-referencing issues

            // Configure Wallet relationship
            modelBuilder.Entity<Wallet>()
                .HasRequired(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .WillCascadeOnDelete(true); // Enable cascade delete for User -> Wallet

            // Configure CardDetails relationship
            modelBuilder.Entity<CardDetails>()
                .HasRequired(cd => cd.User)
                .WithMany()
                .HasForeignKey(cd => cd.UserId)
                .WillCascadeOnDelete(true); // Enable cascade delete for User -> CardDetails

            // Configure PrintingRequest relationships
            modelBuilder.Entity<PrintingRequest>()
                .HasRequired(pr => pr.User)
                .WithMany()
                .HasForeignKey(pr => pr.UserId)
                .WillCascadeOnDelete(false); // Disable cascade delete to prevent cycles

            modelBuilder.Entity<PrintingRequest>()
                .HasRequired(pr => pr.Book)
                .WithMany()
                .HasForeignKey(pr => pr.BookId)
                .WillCascadeOnDelete(false); // Disable cascade delete

            modelBuilder.Entity<PrintingRequest>()
                .HasOptional(pr => pr.AssignedEmployee)
                .WithMany()
                .HasForeignKey(pr => pr.AssignedEmployeeId)
                .WillCascadeOnDelete(false); // Disable cascade delete

            modelBuilder.Entity<PrintingRequest>()
                .HasOptional(pr => pr.AssignedSeller)
                .WithMany()
                .HasForeignKey(pr => pr.AssignedSellerId)
                .WillCascadeOnDelete(false); // Disable cascade delete

            // Configure WithdrawalRequest relationships
            modelBuilder.Entity<WithdrawalRequest>()
                .HasRequired(wr => wr.User)
                .WithMany()
                .HasForeignKey(wr => wr.UserId)
                .WillCascadeOnDelete(false); // Disable cascade delete to prevent cycles

            modelBuilder.Entity<WithdrawalRequest>()
                .HasRequired(wr => wr.BankAccount)
                .WithMany()
                .HasForeignKey(wr => wr.BankAccountId)
                .WillCascadeOnDelete(false); // Disable cascade delete

            modelBuilder.Entity<WithdrawalRequest>()
                .HasOptional(wr => wr.Transaction)
                .WithMany()
                .HasForeignKey(wr => wr.TransactionId)
                .WillCascadeOnDelete(false); // Disable cascade delete
        }
    }
}