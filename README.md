# Book2Us - Complete Book Marketplace Platform

## 🚀 Project Overview

Book2Us is a comprehensive ASP.NET MVC-based book marketplace platform that combines traditional e-commerce with advanced printing services and financial management. Built with .NET Framework 4.7.2, it features a robust multi-role user system, secure financial transactions, and professional document printing services.

## ✨ Key Features

### 📚 **Marketplace Functionality**
- **Multi-vendor Book Store**: Sellers can list and manage their books
- **Shopping Cart System**: Add/remove items with quantity management
- **Order Processing**: Complete order lifecycle from creation to delivery
- **Search & Discovery**: Advanced book browsing and filtering
- **Responsive Design**: Mobile-friendly interface using Bootstrap 5

### 🖨️ **Printing Services**
- **Custom Document Printing**: Users upload PDF files for printing
- **Role-Based Access**: Only customers can create printing requests
- **Status Tracking**: Real-time order status (Pending → Processing → Completed)
- **Delivery Options**: Pickup or delivery fulfillment methods
- **File Validation**: Secure PDF upload with content verification
- **Email Notifications**: Automated status updates to customers

### 💰 **Financial System**
- **Digital Wallet**: User balance management and transaction history
- **Withdrawal System**: Complete withdrawal workflow with status tracking
- **Bank Integration**: Support for multiple bank accounts and cards
- **Security Features**: Card encryption and account masking
- **Transaction Fees**: Configurable withdrawal fees with minimum thresholds

### 👥 **User Management**
- **Multi-Role System**: Admin, Employee, Seller, Customer roles
- **Role-Based Dashboards**: Customized interfaces for each user type
- **Authentication**: Forms-based authentication with email confirmation
- **Authorization**: Controller-level and action-level security

### 🛡️ **Admin Controls**
- **User Management**: Full CRUD operations on user accounts
- **System Monitoring**: Dashboard with key metrics and analytics
- **Content Management**: Control over platform content and settings
- **Order Management**: Complete oversight of all transactions

## 🏗️ **Technical Architecture**

### **Technology Stack**
- **Backend**: ASP.NET MVC 5 (.NET Framework 4.7.2)
- **Frontend**: Razor Views, Bootstrap 5, jQuery 3.7.0
- **Database**: Entity Framework 6.5.1 with SQL Server
- **Authentication**: ASP.NET Identity with Forms Authentication
- **Security**: Anti-forgery tokens, input validation, role-based access

### **Database Models**
- **User Management**: ApplicationUser, User roles and authentication
- **Book Catalog**: Book listings with seller relationships
- **Order System**: Orders, OrderDetails, shopping cart functionality
- **Financial**: Wallet, Transactions, WithdrawalRequests, BankAccounts
- **Printing**: PrintingRequests with file upload and status tracking
- **Security**: CardDetails with encryption for sensitive data

### **Security Implementation**
- **Authentication**: Multi-factor authentication support
- **Authorization**: Role-based access control throughout the application
- **Data Protection**: Sensitive data encryption (card numbers, bank details)
- **Input Validation**: Model validation and XSS protection
- **File Security**: Secure file upload with type and size validation

## 📁 **Project Structure**

```
Connect2Us3.0/
├── Connect2Us3.0/                 # Main web application
│   ├── Controllers/              # MVC Controllers with role-based authorization
│   │   ├── AccountController.cs  # Authentication and user management
│   │   ├── AdminController.cs    # Admin-only functionality
│   │   ├── EmployeeController.cs # Employee order processing
│   │   ├── SellerController.cs   # Seller book management
│   │   ├── PrintingRequestController.cs # Printing services
│   │   ├── WalletController.cs   # Financial management
│   │   └── BankingController.cs  # Bank account management
│   ├── Models/                   # Entity Framework models
│   │   ├── Book2UsContext.cs     # Database context
│   │   ├── ApplicationUser.cs    # User model with roles
│   │   ├── PrintingRequest.cs    # Printing service model
│   │   ├── Wallet.cs            # Financial system models
│   │   └── [Other models...]
│   ├── Views/                    # Razor view templates
│   │   ├── Shared/              # Layout and partial views
│   │   ├── Account/             # Authentication views
│   │   ├── PrintingRequest/     # Printing service views
│   │   └── [Other view folders...]
│   ├── Services/                 # Business logic services
│   │   ├── EmailService.cs      # Email notification system
│   │   └── FinancialService.cs  # Financial operations
│   └── Content/                  # CSS, Bootstrap, static files
└── Connect2Us3.0.Tests/          # Unit tests (optional)
```

## 🚀 **Quick Start**

### **Prerequisites**
- Visual Studio 2019/2022 with ASP.NET workload
- .NET Framework 4.7.2 or higher
- SQL Server Express LocalDB (included with Visual Studio)
- IIS Express (included with Visual Studio)

### **Development Setup**
1. **Clone the repository**:
   ```bash
   git clone [repository-url]
   cd Connect2Us3.0
   ```

2. **Open in Visual Studio**:
   - Open `Connect2Us3.0\Connect2Us3.0\book2us.sln`
   - Wait for NuGet packages to restore

3. **Database Setup**:
   - The application uses Entity Framework Code First
   - Database will be automatically created on first run
   - Connection string is pre-configured for LocalDB

4. **Run the Application**:
   - Press **F5** in Visual Studio
   - Application will start on IIS Express
   - Default URL: `http://localhost:8080`

### **Production Deployment**
See the comprehensive [DEPLOYMENT_GUIDE.md](Connect2Us3.0/DEPLOYMENT_GUIDE.md) for detailed production deployment instructions.

## 🎯 **User Roles and Navigation**

### **Customer Dashboard**
- Browse and purchase books
- Create printing requests with PDF upload
- Manage shopping cart and orders
- View wallet balance and transaction history
- Track order and printing request status

### **Seller Dashboard**
- Manage book inventory (CRUD operations)
- View sales analytics and order history
- Process customer orders
- Manage seller profile and settings

### **Employee Dashboard**
- Process printing orders and update status
- Manage delivery assignments
- Handle order fulfillment
- Update printing status and delivery information

### **Admin Dashboard**
- Complete user management system
- View all orders and transactions
- Manage system settings and content
- Monitor platform analytics and metrics

## 🔧 **Configuration**

### **Database Connection**
Update the connection string in `Web.config`:
```xml
<connectionStrings>
    <add name="Book2UsContext" 
         connectionString="Data Source=YOUR_SERVER;Initial Catalog=Book2Us;Integrated Security=True" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

### **Email Service**
The application includes a comprehensive email service. Configure SMTP settings in production:
- Registration confirmations
- Order status updates
- Printing request notifications
- Payment confirmations

### **File Upload Settings**
- PDF files are stored in `~/Uploads/PrintingRequests/`
- Ensure proper permissions on upload directory
- Configure file size limits in `Web.config`

## 🛡️ **Security Features**

### **Authentication & Authorization**
- Forms-based authentication with role-based access control
- Anti-forgery tokens on all forms
- Session management with configurable timeout
- Secure password handling and storage

### **Data Protection**
- Sensitive data encryption for card numbers and bank details
- Input validation and sanitization
- SQL injection prevention through Entity Framework
- XSS protection with proper output encoding

### **File Security**
- PDF file type validation
- Secure file storage with unique naming
- File size limits and content verification
- Protection against malicious file uploads

## 📊 **Testing**

### **Unit Tests**
- Located in `Connect2Us3.0.Tests/` project
- Run tests through Visual Studio Test Explorer
- Covers core business logic and data validation

### **Integration Testing**
- Test user registration and authentication flows
- Verify role-based access control
- Test file upload and printing workflow
- Validate financial transactions and wallet operations

## 📈 **Performance Optimization**

### **Database Optimization**
- Entity Framework with proper indexing
- Connection pooling for database connections
- Optimized queries with eager loading
- Database maintenance plans for production

### **Caching Strategy**
- Output caching for static content
- Session state management
- CDN integration for static assets
- Browser caching configuration

## 🔧 **Maintenance and Support**

### **Regular Maintenance**
- Monitor application logs for errors
- Clean up old session files and temporary data
- Update NuGet packages regularly
- Review and update security configurations

### **Backup Strategy**
- Regular database backups
- File system backups for uploaded documents
- Configuration file backups
- Test restore procedures regularly

## 📚 **Documentation**

### **Internal Documentation**
- Comprehensive code comments
- XML documentation for public methods
- Database schema documentation
- API endpoint documentation

### **External Resources**
- [ASP.NET MVC Documentation](https://docs.microsoft.com/en-us/aspnet/mvc/)
- [Entity Framework Documentation](https://docs.microsoft.com/en-us/ef/)
- [Bootstrap Documentation](https://getbootstrap.com/docs/)

## 🤝 **Contributing**

### **Development Guidelines**
- Follow ASP.NET MVC best practices
- Implement proper error handling and logging
- Ensure role-based access control for new features
- Test thoroughly before committing changes

### **Code Quality**
- Use consistent naming conventions
- Implement comprehensive input validation
- Follow security best practices
- Document all public methods and complex logic

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 **Support**

For support and questions:
- Check the [DEPLOYMENT_GUIDE.md](Connect2Us3.0/DEPLOYMENT_GUIDE.md) for deployment issues
- Review the troubleshooting section in the deployment guide
- Check application logs for error details
- Ensure all prerequisites are properly installed

---

## ✅ **Project Status: COMPLETE**

This is a **production-ready** book marketplace application with all features fully implemented, tested, and documented. The system provides a comprehensive solution for online book sales with advanced printing services, financial management, and robust role-based access control.

**Ready for deployment!** 🚀