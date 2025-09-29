using book2us.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System; 
using System.Collections.Generic;
using System.Data.Entity;

namespace book2us.DAL
{
    public class Book2UsInitializer : DropCreateDatabaseIfModelChanges<Book2UsContext>
    {
        protected override void Seed(Book2UsContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<User>(new UserStore<User>(context));

            // Create roles
            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }
            if (!roleManager.RoleExists("Seller"))
            {
                roleManager.Create(new IdentityRole("Seller"));
            }
            if (!roleManager.RoleExists("Employee"))
            {
                roleManager.Create(new IdentityRole("Employee"));
            }
            if (!roleManager.RoleExists("Customer"))
            {
                roleManager.Create(new IdentityRole("Customer"));
            }

            // Create users
            var adminUser = new User { UserName = "admin@book2us.com", Email = "admin@book2us.com" };
            var sellerUser = new User { UserName = "seller@book2us.com", Email = "seller@book2us.com" };
            var employeeUser = new User { UserName = "employee@book2us.com", Email = "employee@book2us.com" };
            var customerUser = new User { UserName = "customer@book2us.com", Email = "customer@book2us.com" };

            userManager.Create(adminUser, "Password123!");
            userManager.Create(sellerUser, "Password123!");
            userManager.Create(employeeUser, "Password123!");
            userManager.Create(customerUser, "Password123!");

            // Assign roles to users
            userManager.AddToRole(adminUser.Id, "Admin");
            userManager.AddToRole(sellerUser.Id, "Seller");
            userManager.AddToRole(employeeUser.Id, "Employee");
            userManager.AddToRole(customerUser.Id, "Customer");

            // Create books
            var books = new List<Book>
            {
                new Book { Title = "The Lord of the Rings", Author = "J.R.R. Tolkien", Genre = "Fantasy", Price = 25.00m, Stock = 10, SellerId = sellerUser.Id },
                new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien", Genre = "Fantasy", Price = 20.00m, Stock = 15, SellerId = sellerUser.Id },
                new Book { Title = "A Game of Thrones", Author = "George R.R. Martin", Genre = "Fantasy", Price = 30.00m, Stock = 5, SellerId = sellerUser.Id },
            };

            books.ForEach(b => context.Books.Add(b));
            context.SaveChanges();
        }
    }
}