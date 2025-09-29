using System.Data.Entity;

namespace book2us.Models
{
    public class Book2UsContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<PrintingRequest> PrintingRequests { get; set; }
    }
}