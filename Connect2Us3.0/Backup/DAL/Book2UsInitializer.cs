using book2us.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace book2us.DAL
{
    public class Book2UsInitializer : DropCreateDatabaseIfModelChanges<Book2UsContext>
    {
        protected override void Seed(Book2UsContext context)
        {
            // Check if we already have users
            if (context.Users.Any())
            {
                return; // Database has been seeded
            }

            // Create users with different roles
            var adminUser = new User { Username = "admin@book2us.com", Email = "admin@book2us.com", Role = "Admin" };
            var sellerUser = new User { Username = "seller@book2us.com", Email = "seller@book2us.com", Role = "Seller" };
            var employeeUser = new User { Username = "employee@book2us.com", Email = "employee@book2us.com", Role = "Employee" };
            var customerUser = new User { Username = "customer@book2us.com", Email = "customer@book2us.com", Role = "Customer" };

            // Add users to context (without password hashing for simplicity)
            context.Users.Add(adminUser);
            context.Users.Add(sellerUser);
            context.Users.Add(employeeUser);
            context.Users.Add(customerUser);
            context.SaveChanges();

            // Create books
            var books = new List<Book>
            {
                new Book { Title = "The Lord of the Rings", Author = "J.R.R. Tolkien", Description = "Epic fantasy adventure", Price = 25.00m, SellerId = sellerUser.Id, SellerUserName = sellerUser.Username },
                new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien", Description = "Fantasy adventure prequel", Price = 20.00m, SellerId = sellerUser.Id, SellerUserName = sellerUser.Username },
                new Book { Title = "A Game of Thrones", Author = "George R.R. Martin", Description = "Epic fantasy political intrigue", Price = 30.00m, SellerId = sellerUser.Id, SellerUserName = sellerUser.Username },
            };

            books.ForEach(b => context.Books.Add(b));
            context.SaveChanges();
        }
    }
}