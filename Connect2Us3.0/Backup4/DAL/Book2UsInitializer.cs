using book2us.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace book2us.DAL
{
    public class Book2UsInitializer : DropCreateDatabaseAlways<Book2UsContext>
    {
        protected override void Seed(Book2UsContext context)
        {
            // Check if we already have users
            if (context.ApplicationUsers.Any())
            {
                return; // Database has been seeded
            }

            // Create users with different roles and profile information
            var adminUser = new ApplicationUser { 
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
            };
            
            var sellerUser = new ApplicationUser { 
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
            };
            
            var employeeUser = new ApplicationUser { 
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
            };
            
            var customerUser = new ApplicationUser { 
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
            };

            // Add users to context (without password hashing for simplicity)
            context.ApplicationUsers.Add(adminUser);
            context.ApplicationUsers.Add(sellerUser);
            context.ApplicationUsers.Add(employeeUser);
            context.ApplicationUsers.Add(customerUser);
            context.SaveChanges();

            // Create books
            var books = new List<Book>
            {
                new Book { Title = "The Lord of the Rings", Author = "J.R.R. Tolkien", Description = "Epic fantasy adventure", Price = 25.00m, SellerId = sellerUser.Id, SellerUserName = sellerUser.UserName },
                new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien", Description = "Fantasy adventure prequel", Price = 20.00m, SellerId = sellerUser.Id, SellerUserName = sellerUser.UserName },
                new Book { Title = "A Game of Thrones", Author = "George R.R. Martin", Description = "Epic fantasy political intrigue", Price = 30.00m, SellerId = sellerUser.Id, SellerUserName = sellerUser.UserName },
            };

            books.ForEach(b => context.Books.Add(b));
            context.SaveChanges();
        }
    }
}