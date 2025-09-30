-- Book2Us Database Migration Script
-- This script creates the complete database schema for the Book2Us application
-- Version: 1.0
-- Created for deployment to production environments

-- Create the database (uncomment if you need to create the database)
-- CREATE DATABASE Book2UsDB;
-- GO

-- Use the database
-- USE Book2UsDB;
-- GO

-- =============================================
-- Users and Authentication Tables
-- =============================================

-- ApplicationUsers Table
CREATE TABLE [dbo].[ApplicationUsers] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserName] NVARCHAR(256) NOT NULL UNIQUE,
    [Email] NVARCHAR(256) NOT NULL,
    [Password] NVARCHAR(256) NOT NULL,
    [Role] NVARCHAR(50) NOT NULL DEFAULT 'Customer',
    [FirstName] NVARCHAR(100) NULL,
    [LastName] NVARCHAR(100) NULL,
    [Address] NVARCHAR(500) NULL,
    [City] NVARCHAR(100) NULL,
    [State] NVARCHAR(50) NULL,
    [PostalCode] NVARCHAR(20) NULL,
    [Country] NVARCHAR(100) NULL,
    [Phone] NVARCHAR(50) NULL,
    [Gender] NVARCHAR(10) NULL,
    [Age] INT NULL,
    [ProfilePicture] NVARCHAR(MAX) NULL,
    [ResetPasswordToken] NVARCHAR(256) NULL,
    [ResetPasswordExpiry] DATETIME NULL
);

-- Users Table (if needed for legacy compatibility)
CREATE TABLE [dbo].[Users] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserName] NVARCHAR(256) NOT NULL UNIQUE,
    [Email] NVARCHAR(256) NOT NULL,
    [Password] NVARCHAR(256) NOT NULL,
    [Role] NVARCHAR(50) NOT NULL DEFAULT 'Customer'
);

-- =============================================
-- Book Management Tables
-- =============================================

-- Books Table
CREATE TABLE [dbo].[Books] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Title] NVARCHAR(500) NOT NULL,
    [Author] NVARCHAR(500) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Price] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    [SellerId] INT NOT NULL,
    [SellerUserName] NVARCHAR(256) NULL,
    [ImageUrl] NVARCHAR(500) NULL,
    [Category] NVARCHAR(100) NULL,
    [ISBN] NVARCHAR(20) NULL,
    [Condition] NVARCHAR(50) NULL DEFAULT 'New',
    [IsAvailable] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [ModifiedDate] DATETIME NULL,
    CONSTRAINT [FK_Books_ApplicationUsers] FOREIGN KEY ([SellerId]) REFERENCES [dbo].[ApplicationUsers] ([Id]) ON DELETE CASCADE
);

-- =============================================
-- Order Management Tables
-- =============================================

-- Orders Table
CREATE TABLE [dbo].[Orders] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [OrderDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [Username] NVARCHAR(256) NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [Address] NVARCHAR(500) NOT NULL,
    [City] NVARCHAR(100) NOT NULL,
    [State] NVARCHAR(50) NOT NULL,
    [PostalCode] NVARCHAR(20) NOT NULL,
    [Country] NVARCHAR(100) NOT NULL,
    [Phone] NVARCHAR(50) NOT NULL,
    [Email] NVARCHAR(256) NOT NULL,
    [Total] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    [PaymentMethod] NVARCHAR(50) NULL DEFAULT 'CreditCard',
    [PaymentTransactionId] NVARCHAR(100) NULL,
    [PaymentStatus] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [OrderStatus] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [HasPrintingService] BIT NOT NULL DEFAULT 0,
    [PrintingCost] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    [DeliveryMethod] NVARCHAR(50) NULL,
    [DeliveryCost] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    [TotalPages] INT NULL DEFAULT 0,
    [FulfillmentMethod] NVARCHAR(50) NULL,
    [CustomerId] INT NULL,
    CONSTRAINT [FK_Orders_ApplicationUsers] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[ApplicationUsers] ([Id])
);

-- OrderDetails Table
CREATE TABLE [dbo].[OrderDetails] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [OrderId] INT NOT NULL,
    [BookId] INT NOT NULL,
    [Quantity] INT NOT NULL DEFAULT 1,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    CONSTRAINT [FK_OrderDetails_Orders] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderDetails_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books] ([Id])
);

-- =============================================
-- Financial Management Tables
-- =============================================

-- Wallets Table
CREATE TABLE [dbo].[Wallets] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] INT NOT NULL,
    [Balance] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [LastModified] DATETIME NULL,
    CONSTRAINT [FK_Wallets_ApplicationUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]) ON DELETE CASCADE
);

-- BankAccounts Table
CREATE TABLE [dbo].[BankAccounts] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] INT NOT NULL,
    [AccountHolderName] NVARCHAR(200) NOT NULL,
    [BankName] NVARCHAR(200) NOT NULL,
    [AccountNumber] NVARCHAR(100) NOT NULL,
    [RoutingNumber] NVARCHAR(50) NULL,
    [IBAN] NVARCHAR(50) NULL,
    [SWIFTCode] NVARCHAR(50) NULL,
    [AccountType] NVARCHAR(50) NOT NULL DEFAULT 'Checking',
    [IsVerified] BIT NOT NULL DEFAULT 0,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_BankAccounts_ApplicationUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]) ON DELETE CASCADE
);

-- CardDetails Table
CREATE TABLE [dbo].[CardDetails] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] INT NOT NULL,
    [CardHolderName] NVARCHAR(200) NOT NULL,
    [CardNumber] NVARCHAR(100) NOT NULL,
    [ExpiryMonth] INT NOT NULL,
    [ExpiryYear] INT NOT NULL,
    [CVV] NVARCHAR(10) NOT NULL,
    [CardType] NVARCHAR(50) NOT NULL,
    [IsVerified] BIT NOT NULL DEFAULT 0,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_CardDetails_ApplicationUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]) ON DELETE CASCADE
);

-- Transactions Table
CREATE TABLE [dbo].[Transactions] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] INT NOT NULL,
    [WalletId] INT NOT NULL,
    [TransactionType] NVARCHAR(50) NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [ReferenceId] NVARCHAR(100) NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Completed',
    [TransactionDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_Transactions_ApplicationUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_Transactions_Wallets] FOREIGN KEY ([WalletId]) REFERENCES [dbo].[Wallets] ([Id])
);

-- WithdrawalRequests Table
CREATE TABLE [dbo].[WithdrawalRequests] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] INT NOT NULL,
    [BankAccountId] INT NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [RequestDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [ProcessedDate] DATETIME NULL,
    [ProcessedBy] NVARCHAR(256) NULL,
    [Notes] NVARCHAR(500) NULL,
    CONSTRAINT [FK_WithdrawalRequests_ApplicationUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[ApplicationUsers] ([Id]),
    CONSTRAINT [FK_WithdrawalRequests_BankAccounts] FOREIGN KEY ([BankAccountId]) REFERENCES [dbo].[BankAccounts] ([Id])
);

-- =============================================
-- Printing Service Tables
-- =============================================

-- PrintingRequests Table
CREATE TABLE [dbo].[PrintingRequests] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [OrderId] INT NOT NULL,
    [CustomerId] INT NOT NULL,
    [DocumentTitle] NVARCHAR(500) NOT NULL,
    [TotalPages] INT NOT NULL,
    [ColorPages] INT NOT NULL DEFAULT 0,
    [BlackWhitePages] INT NOT NULL DEFAULT 0,
    [FulfillmentMethod] NVARCHAR(50) NOT NULL,
    [DeliveryAddress] NVARCHAR(500) NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [Cost] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    [RequestedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [CompletedDate] DATETIME NULL,
    [ProcessedBy] NVARCHAR(256) NULL,
    [Notes] NVARCHAR(MAX) NULL,
    CONSTRAINT [FK_PrintingRequests_Orders] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]),
    CONSTRAINT [FK_PrintingRequests_ApplicationUsers] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[ApplicationUsers] ([Id])
);

-- =============================================
-- Create Indexes for Performance
-- =============================================

-- User indexes
CREATE INDEX [IX_ApplicationUsers_UserName] ON [dbo].[ApplicationUsers] ([UserName]);
CREATE INDEX [IX_ApplicationUsers_Email] ON [dbo].[ApplicationUsers] ([Email]);
CREATE INDEX [IX_ApplicationUsers_Role] ON [dbo].[ApplicationUsers] ([Role]);

-- Book indexes
CREATE INDEX [IX_Books_SellerId] ON [dbo].[Books] ([SellerId]);
CREATE INDEX [IX_Books_Title] ON [dbo].[Books] ([Title]);
CREATE INDEX [IX_Books_Author] ON [dbo].[Books] ([Author]);
CREATE INDEX [IX_Books_IsAvailable] ON [dbo].[Books] ([IsAvailable]);

-- Order indexes
CREATE INDEX [IX_Orders_Username] ON [dbo].[Orders] ([Username]);
CREATE INDEX [IX_Orders_CustomerId] ON [dbo].[Orders] ([CustomerId]);
CREATE INDEX [IX_Orders_OrderDate] ON [dbo].[Orders] ([OrderDate]);
CREATE INDEX [IX_Orders_OrderStatus] ON [dbo].[Orders] ([OrderStatus]);
CREATE INDEX [IX_Orders_PaymentStatus] ON [dbo].[Orders] ([PaymentStatus]);

-- OrderDetail indexes
CREATE INDEX [IX_OrderDetails_OrderId] ON [dbo].[OrderDetails] ([OrderId]);
CREATE INDEX [IX_OrderDetails_BookId] ON [dbo].[OrderDetails] ([BookId]);

-- Financial indexes
CREATE INDEX [IX_Wallets_UserId] ON [dbo].[Wallets] ([UserId]);
CREATE INDEX [IX_BankAccounts_UserId] ON [dbo].[BankAccounts] ([UserId]);
CREATE INDEX [IX_CardDetails_UserId] ON [dbo].[CardDetails] ([UserId]);
CREATE INDEX [IX_Transactions_UserId] ON [dbo].[Transactions] ([UserId]);
CREATE INDEX [IX_Transactions_WalletId] ON [dbo].[Transactions] ([WalletId]);
CREATE INDEX [IX_Transactions_TransactionDate] ON [dbo].[Transactions] ([TransactionDate]);
CREATE INDEX [IX_WithdrawalRequests_UserId] ON [dbo].[WithdrawalRequests] ([UserId]);
CREATE INDEX [IX_WithdrawalRequests_Status] ON [dbo].[WithdrawalRequests] ([Status]);

-- Printing indexes
CREATE INDEX [IX_PrintingRequests_OrderId] ON [dbo].[PrintingRequests] ([OrderId]);
CREATE INDEX [IX_PrintingRequests_CustomerId] ON [dbo].[PrintingRequests] ([CustomerId]);
CREATE INDEX [IX_PrintingRequests_Status] ON [dbo].[PrintingRequests] ([Status]);

PRINT 'Database schema created successfully!';