using System.Data.Entity.Migrations;
using book2us.Models;

namespace book2us.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Book2UsContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
            ContextKey = "book2us.Models.Book2UsContext";
        }

        protected override void Seed(Book2UsContext context)
        {
            // This method will be called after migrating to the latest version.
            // You can use the DbSet<T>.AddOrUpdate() helper extension method
            // to avoid creating duplicate seed data.

            // Seed initial users
            SeedUsers(context);
            SeedBooks(context);
            SeedWallets(context);
            SeedBankAccounts(context);
        }

        private void SeedUsers(Book2UsContext context)
        {
            // Admin User
            if (!context.ApplicationUsers.Any(u => u.UserName == "admin@book2us.com"))
            {
                context.ApplicationUsers.Add(new ApplicationUser
                {
                    UserName = "admin@book2us.com",
                    Email = "admin@book2us.com",
                    Password = "Admin123!",
                    Role = "Admin",
                    FirstName = "Admin",
                    LastName = "User",
                    Address = "123 Admin Street",
                    City = "Admin City",
                    State = "CA",
                    PostalCode = "12345",
                    Country = "USA",
                    Phone = "555-0101"
                });
            }

            // Seller User
            if (!context.ApplicationUsers.Any(u => u.UserName == "seller@book2us.com"))
            {
                context.ApplicationUsers.Add(new ApplicationUser
                {
                    UserName = "seller@book2us.com",
                    Email = "seller@book2us.com",
                    Password = "Seller123!",
                    Role = "Seller",
                    FirstName = "Seller",
                    LastName = "User",
                    Address = "456 Seller Avenue",
                    City = "Seller Town",
                    State = "NY",
                    PostalCode = "67890",
                    Country = "USA",
                    Phone = "555-0102"
                });
            }

            // Employee User
            if (!context.ApplicationUsers.Any(u => u.UserName == "employee@book2us.com"))
            {
                context.ApplicationUsers.Add(new ApplicationUser
                {
                    UserName = "employee@book2us.com",
                    Email = "employee@book2us.com",
                    Password = "Employee123!",
                    Role = "Employee",
                    FirstName = "Employee",
                    LastName = "User",
                    Address = "789 Employee Boulevard",
                    City = "Employee City",
                    State = "TX",
                    PostalCode = "54321",
                    Country = "USA",
                    Phone = "555-0103"
                });
            }

            // Customer User
            if (!context.ApplicationUsers.Any(u => u.UserName == "customer@book2us.com"))
            {
                context.ApplicationUsers.Add(new ApplicationUser
                {
                    UserName = "customer@book2us.com",
                    Email = "customer@book2us.com",
                    Password = "Customer123!",
                    Role = "Customer",
                    FirstName = "John",
                    LastName = "Doe",
                    Address = "321 Customer Lane",
                    City = "Customer City",
                    State = "FL",
                    PostalCode = "98765",
                    Country = "USA",
                    Phone = "555-0104"
                });
            }

            context.SaveChanges();
        }

        private void SeedBooks(Book2UsContext context)
        {
            var sellerUser = context.ApplicationUsers.FirstOrDefault(u => u.UserName == "seller@book2us.com");
            if (sellerUser != null && !context.Books.Any())
            {
                context.Books.AddRange(new[]
                {
                    new Book
                    {
                        Title = "The Lord of the Rings",
                        Author = "J.R.R. Tolkien",
                        Description = "Epic fantasy adventure",
                        Price = 25.00m,
                        SellerId = sellerUser.Id,
                        SellerUserName = sellerUser.UserName,
                        Category = "Fantasy",
                        ISBN = "978-0544003415",
                        Condition = "New",
                        IsAvailable = true
                    },
                    new Book
                    {
                        Title = "The Hobbit",
                        Author = "J.R.R. Tolkien",
                        Description = "Fantasy adventure prequel",
                        Price = 20.00m,
                        SellerId = sellerUser.Id,
                        SellerUserName = sellerUser.UserName,
                        Category = "Fantasy",
                        ISBN = "978-0547928227",
                        Condition = "New",
                        IsAvailable = true
                    },
                    new Book
                    {
                        Title = "A Game of Thrones",
                        Author = "George R.R. Martin",
                        Description = "Epic fantasy political intrigue",
                        Price = 30.00m,
                        SellerId = sellerUser.Id,
                        SellerUserName = sellerUser.UserName,
                        Category = "Fantasy",
                        ISBN = "978-0553593716",
                        Condition = "New",
                        IsAvailable = true
                    }
                });

                context.SaveChanges();
            }
        }

        private void SeedWallets(Book2UsContext context)
        {
            var users = context.ApplicationUsers.ToList();
            foreach (var user in users)
            {
                if (!context.Wallets.Any(w => w.UserId == user.Id))
                {
                    decimal initialBalance = user.Role switch
                    {
                        "Admin" => 1000.00m,
                        "Seller" => 500.00m,
                        "Employee" => 250.00m,
                        _ => 200.00m // Customer
                    };

                    context.Wallets.Add(new Wallet
                    {
                        UserId = user.Id,
                        Balance = initialBalance,
                        CreatedDate = DateTime.Now
                    });
                }
            }
            context.SaveChanges();
        }

        private void SeedBankAccounts(Book2UsContext context)
        {
            var sellerUser = context.ApplicationUsers.FirstOrDefault(u => u.UserName == "seller@book2us.com");
            var customerUser = context.ApplicationUsers.FirstOrDefault(u => u.UserName == "customer@book2us.com");

            if (sellerUser != null && !context.BankAccounts.Any(ba => ba.UserId == sellerUser.Id))
            {
                context.BankAccounts.Add(new BankAccount
                {
                    UserId = sellerUser.Id,
                    AccountHolderName = "Seller User",
                    BankName = "Bank of America",
                    AccountNumber = "1234567890",
                    RoutingNumber = "021000021",
                    AccountType = "Checking",
                    IsVerified = true,
                    CreatedDate = DateTime.Now
                });
            }

            if (customerUser != null && !context.BankAccounts.Any(ba => ba.UserId == customerUser.Id))
            {
                context.BankAccounts.Add(new BankAccount
                {
                    UserId = customerUser.Id,
                    AccountHolderName = "John Doe",
                    BankName = "Chase Bank",
                    AccountNumber = "9876543210",
                    RoutingNumber = "021000021",
                    AccountType = "Savings",
                    IsVerified = true,
                    CreatedDate = DateTime.Now
                });
            }

            context.SaveChanges();
        }
    }
}