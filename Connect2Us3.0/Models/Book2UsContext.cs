using System.Data.Entity;

namespace book2us.Models
{
    public class Book2UsContext : DbContext
    {
        public Book2UsContext() : base("name=Book2UsContext")
        {
        }

        public DbSet<User> Users { get; set; }
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
    }
}