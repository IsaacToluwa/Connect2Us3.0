# Connect2Us3.0

Connect2Us3.0 is a web application for a bookstore that allows users to buy and sell books. It includes features for managing orders, a wallet system for payments, email notifications, and a printing request system for sellers.

## Features

- **User Roles:** The application supports multiple user roles, including Customer, Seller, Employee, and Admin.
- **Book Marketplace:** Users can browse and purchase books from various sellers.
- **Shopping Cart:** A fully functional shopping cart for a seamless checkout experience.
- **Order Management:** Customers can view their order history, and employees can manage order fulfillment.
- **Wallet System:** Users have a wallet to store funds and make payments for orders.
- **Email Notifications:** Automated email notifications for order confirmations.
- **Printing Requests:** Sellers can request printing services for their books.
- **Admin, Seller, and Employee Dashboards:** Dedicated dashboards for managing users, books, and orders.

## Technologies Used

- **ASP.NET MVC 5:** The web application is built using the ASP.NET MVC 5 framework.
- **Entity Framework 6:** Used for data access and management.
- **Bootstrap:** For a responsive and modern user interface.
- **SQL Server:** The database for storing application data.

## Setup and Installation

1. **Clone the repository:**
   ```
   git clone <repository-url>
   ```
2. **Open the solution in Visual Studio:**
   - Open `Connect2Us3.0.sln` in Visual Studio.
3. **Update the database connection string:**
   - In `Web.config`, update the `Book2UsContext` connection string with your SQL Server details.
4. **Run database migrations:**
   - Open the Package Manager Console in Visual Studio.
   - Run the following commands:
     ```
     Enable-Migrations
     Add-Migration InitialCreate
     Update-Database
     ```
5. **Build and run the application:**
   - Press `F5` or click the "Run" button in Visual Studio.

## User Roles

- **Customer:** Can browse books, add them to the cart, and make purchases.
- **Seller:** Can list books for sale, manage their inventory, and request printing services.
- **Employee:** Can manage customer orders and deliveries.
- **Admin:** Has full access to the application and can manage users, books, and all other aspects of the system.