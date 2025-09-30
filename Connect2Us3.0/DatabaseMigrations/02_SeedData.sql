-- Book2Us Database Seed Data Script
-- This script populates the database with initial test data
-- Version: 1.0
-- Run this after creating the database schema

-- =============================================
-- Seed Application Users
-- =============================================

-- Admin User
IF NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'admin@book2us.com')
BEGIN
    INSERT INTO [dbo].[ApplicationUsers] ([UserName], [Email], [Password], [Role], [FirstName], [LastName], [Address], [City], [State], [PostalCode], [Country], [Phone])
    VALUES ('admin@book2us.com', 'admin@book2us.com', 'Admin123!', 'Admin', 'Admin', 'User', '123 Admin Street', 'Admin City', 'CA', '12345', 'USA', '555-0101');
END

-- Seller User
IF NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'seller@book2us.com')
BEGIN
    INSERT INTO [dbo].[ApplicationUsers] ([UserName], [Email], [Password], [Role], [FirstName], [LastName], [Address], [City], [State], [PostalCode], [Country], [Phone])
    VALUES ('seller@book2us.com', 'seller@book2us.com', 'Seller123!', 'Seller', 'Seller', 'User', '456 Seller Avenue', 'Seller Town', 'NY', '67890', 'USA', '555-0102');
END

-- Employee User
IF NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'employee@book2us.com')
BEGIN
    INSERT INTO [dbo].[ApplicationUsers] ([UserName], [Email], [Password], [Role], [FirstName], [LastName], [Address], [City], [State], [PostalCode], [Country], [Phone])
    VALUES ('employee@book2us.com', 'employee@book2us.com', 'Employee123!', 'Employee', 'Employee', 'User', '789 Employee Boulevard', 'Employee City', 'TX', '54321', 'USA', '555-0103');
END

-- Customer User
IF NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'customer@book2us.com')
BEGIN
    INSERT INTO [dbo].[ApplicationUsers] ([UserName], [Email], [Password], [Role], [FirstName], [LastName], [Address], [City], [State], [PostalCode], [Country], [Phone])
    VALUES ('customer@book2us.com', 'customer@book2us.com', 'Customer123!', 'Customer', 'John', 'Doe', '321 Customer Lane', 'Customer City', 'FL', '98765', 'USA', '555-0104');
END

-- Additional Customer Users
IF NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'jane.smith@email.com')
BEGIN
    INSERT INTO [dbo].[ApplicationUsers] ([UserName], [Email], [Password], [Role], [FirstName], [LastName], [Address], [City], [State], [PostalCode], [Country], [Phone])
    VALUES ('jane.smith@email.com', 'jane.smith@email.com', 'Jane123!', 'Customer', 'Jane', 'Smith', '123 Oak Street', 'Springfield', 'IL', '62701', 'USA', '555-0201');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'mike.johnson@email.com')
BEGIN
    INSERT INTO [dbo].[ApplicationUsers] ([UserName], [Email], [Password], [Role], [FirstName], [LastName], [Address], [City], [State], [PostalCode], [Country], [Phone])
    VALUES ('mike.johnson@email.com', 'mike.johnson@email.com', 'Mike123!', 'Customer', 'Mike', 'Johnson', '456 Pine Avenue', 'Madison', 'WI', '53703', 'USA', '555-0202');
END

-- =============================================
-- Seed Books
-- =============================================

DECLARE @SellerId INT;
SELECT @SellerId = [Id] FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'seller@book2us.com';

IF @SellerId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Books] WHERE [Title] = 'The Lord of the Rings')
BEGIN
    INSERT INTO [dbo].[Books] ([Title], [Author], [Description], [Price], [SellerId], [SellerUserName], [Category], [ISBN], [Condition], [IsAvailable])
    VALUES ('The Lord of the Rings', 'J.R.R. Tolkien', 'Epic fantasy adventure', 25.00, @SellerId, 'seller@book2us.com', 'Fantasy', '978-0544003415', 'New', 1);
END

IF @SellerId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Books] WHERE [Title] = 'The Hobbit')
BEGIN
    INSERT INTO [dbo].[Books] ([Title], [Author], [Description], [Price], [SellerId], [SellerUserName], [Category], [ISBN], [Condition], [IsAvailable])
    VALUES ('The Hobbit', 'J.R.R. Tolkien', 'Fantasy adventure prequel', 20.00, @SellerId, 'seller@book2us.com', 'Fantasy', '978-0547928227', 'New', 1);
END

IF @SellerId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Books] WHERE [Title] = 'A Game of Thrones')
BEGIN
    INSERT INTO [dbo].[Books] ([Title], [Author], [Description], [Price], [SellerId], [SellerUserName], [Category], [ISBN], [Condition], [IsAvailable])
    VALUES ('A Game of Thrones', 'George R.R. Martin', 'Epic fantasy political intrigue', 30.00, @SellerId, 'seller@book2us.com', 'Fantasy', '978-0553593716', 'New', 1);
END

IF @SellerId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Books] WHERE [Title] = 'Dune')
BEGIN
    INSERT INTO [dbo].[Books] ([Title], [Author], [Description], [Price], [SellerId], [SellerUserName], [Category], [ISBN], [Condition], [IsAvailable])
    VALUES ('Dune', 'Frank Herbert', 'Science fiction masterpiece', 22.00, @SellerId, 'seller@book2us.com', 'Science Fiction', '978-0441013593', 'New', 1);
END

IF @SellerId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Books] WHERE [Title] = '1984')
BEGIN
    INSERT INTO [dbo].[Books] ([Title], [Author], [Description], [Price], [SellerId], [SellerUserName], [Category], [ISBN], [Condition], [IsAvailable])
    VALUES ('1984', 'George Orwell', 'Dystopian classic', 18.00, @SellerId, 'seller@book2us.com', 'Classic', '978-0451524935', 'New', 1);
END

-- =============================================
-- Seed Wallets for Users
-- =============================================

DECLARE @UserId INT;

-- Admin Wallet
SELECT @UserId = [Id] FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'admin@book2us.com';
IF @UserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Wallets] WHERE [UserId] = @UserId)
BEGIN
    INSERT INTO [dbo].[Wallets] ([UserId], [Balance], [CreatedDate])
    VALUES (@UserId, 1000.00, GETDATE());
END

-- Seller Wallet
SELECT @UserId = [Id] FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'seller@book2us.com';
IF @UserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Wallets] WHERE [UserId] = @UserId)
BEGIN
    INSERT INTO [dbo].[Wallets] ([UserId], [Balance], [CreatedDate])
    VALUES (@UserId, 500.00, GETDATE());
END

-- Employee Wallet
SELECT @UserId = [Id] FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'employee@book2us.com';
IF @UserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Wallets] WHERE [UserId] = @UserId)
BEGIN
    INSERT INTO [dbo].[Wallets] ([UserId], [Balance], [CreatedDate])
    VALUES (@UserId, 250.00, GETDATE());
END

-- Customer Wallet
SELECT @UserId = [Id] FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'customer@book2us.com';
IF @UserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Wallets] WHERE [UserId] = @UserId)
BEGIN
    INSERT INTO [dbo].[Wallets] ([UserId], [Balance], [CreatedDate])
    VALUES (@UserId, 200.00, GETDATE());
END

-- =============================================
-- Seed Bank Accounts
-- =============================================

-- Seller Bank Account
SELECT @UserId = [Id] FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'seller@book2us.com';
IF @UserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[BankAccounts] WHERE [UserId] = @UserId)
BEGIN
    INSERT INTO [dbo].[BankAccounts] ([UserId], [AccountHolderName], [BankName], [AccountNumber], [RoutingNumber], [AccountType], [IsVerified], [CreatedDate])
    VALUES (@UserId, 'Seller User', 'Bank of America', '1234567890', '021000021', 'Checking', 1, GETDATE());
END

-- Customer Bank Account
SELECT @UserId = [Id] FROM [dbo].[ApplicationUsers] WHERE [UserName] = 'customer@book2us.com';
IF @UserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[BankAccounts] WHERE [UserId] = @UserId)
BEGIN
    INSERT INTO [dbo].[BankAccounts] ([UserId], [AccountHolderName], [BankName], [AccountNumber], [RoutingNumber], [AccountType], [IsVerified], [CreatedDate])
    VALUES (@UserId, 'John Doe', 'Chase Bank', '9876543210', '021000021', 'Savings', 1, GETDATE());
END

PRINT 'Seed data inserted successfully!';