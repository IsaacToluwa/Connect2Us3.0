using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using book2us.Models;

namespace book2us.Migrations
{
    /// <summary>
    /// Database migration utility for managing database schema and migrations
    /// </summary>
    public class DatabaseMigrationUtility
    {
        private readonly Book2UsContext _context;

        public DatabaseMigrationUtility(Book2UsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Initialize the database with migrations
        /// </summary>
        public void InitializeDatabase()
        {
            try
            {
                // Check if database exists
                if (!Database.Exists(_context.Database.Connection.ConnectionString))
                {
                    Console.WriteLine("Database does not exist. Creating database...");
                    _context.Database.Create();
                    Console.WriteLine("Database created successfully.");
                }

                // Run migrations
                var migrator = new DbMigrator(new Configuration());
                var pendingMigrations = migrator.GetPendingMigrations().ToList();

                if (pendingMigrations.Any())
                {
                    Console.WriteLine($"Found {pendingMigrations.Count} pending migrations:");
                    foreach (var migration in pendingMigrations)
                    {
                        Console.WriteLine($"  - {migration}");
                    }

                    Console.WriteLine("Applying migrations...");
                    migrator.Update();
                    Console.WriteLine("Migrations applied successfully.");
                }
                else
                {
                    Console.WriteLine("Database is up to date. No pending migrations.");
                }

                // Seed data if database is empty
                SeedInitialData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Seed initial data if database is empty
        /// </summary>
        private void SeedInitialData()
        {
            try
            {
                if (!_context.ApplicationUsers.Any())
                {
                    Console.WriteLine("Seeding initial data...");
                    
                    var configuration = new Configuration();
                    configuration.Seed(_context);
                    
                    Console.WriteLine("Initial data seeded successfully.");
                }
                else
                {
                    Console.WriteLine("Database already contains data. Skipping seed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check database connectivity
        /// </summary>
        /// <returns>True if database is accessible, false otherwise</returns>
        public bool TestConnection()
        {
            try
            {
                return _context.Database.Exists();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get database information
        /// </summary>
        public void GetDatabaseInfo()
        {
            try
            {
                Console.WriteLine("=== Database Information ===");
                Console.WriteLine($"Database Exists: {Database.Exists(_context.Database.Connection.ConnectionString)}");
                Console.WriteLine($"Connection String: {_context.Database.Connection.ConnectionString}");
                
                if (Database.Exists(_context.Database.Connection.ConnectionString))
                {
                    Console.WriteLine($"Application Users: {_context.ApplicationUsers.Count()}");
                    Console.WriteLine($"Books: {_context.Books.Count()}");
                    Console.WriteLine($"Orders: {_context.Orders.Count()}");
                    Console.WriteLine($"Wallets: {_context.Wallets.Count()}");
                    Console.WriteLine($"Transactions: {_context.Transactions.Count()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting database info: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset database (drop and recreate)
        /// Use with caution - this will delete all data!
        /// </summary>
        public void ResetDatabase()
        {
            try
            {
                Console.WriteLine("WARNING: This will delete all data in the database!");
                Console.WriteLine("Are you sure you want to continue? (yes/no)");
                
                var response = Console.ReadLine();
                if (response?.ToLower() == "yes")
                {
                    Console.WriteLine("Resetting database...");
                    
                    // Delete all data
                    _context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[OrderDetails]");
                    _context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[Orders]");
                    _context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[PrintingRequests]");
                    _context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[WithdrawalRequests]");
                    _context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[Transactions]");
                    _context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[Wallets]");
                    _context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[BankAccounts]");
                    _context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[CardDetails]");
                    _context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[Books]");
                    _context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[ApplicationUsers]");
                    _context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[Users]");
                    
                    Console.WriteLine("Database reset completed.");
                    
                    // Seed fresh data
                    SeedInitialData();
                }
                else
                {
                    Console.WriteLine("Database reset cancelled.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting database: {ex.Message}");
                throw;
            }
        }
    }
}