using System;
using System.Configuration;
using book2us.Services;
using book2us.Models;

namespace book2us
{
    class TestEmailConsole
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing Email Service...");
            
            try
            {
                var emailService = new EmailService();
                
                Console.WriteLine("1. Testing Payment Confirmation Email...");
                emailService.SendPaymentConfirmation(
                    "customer@example.com", 
                    "John Doe", 
                    99.99m, 
                    "Book Purchase", 
                    "ORD-12345"
                );
                Console.WriteLine("✓ Payment confirmation email sent!");
                
                Console.WriteLine("2. Testing Printing Status Update Email...");
                emailService.SendPrintingStatusUpdate(
                    "customer@example.com",
                    "johndoe",
                    123,
                    "Completed",
                    "Your printing request has been completed and is ready for pickup."
                );
                Console.WriteLine("✓ Printing status update email sent!");
                
                Console.WriteLine("3. Testing Book Seller Notification...");
                emailService.SendBookSellerNotification(
                    "seller@example.com",
                    "selleruser",
                    "ORD-12345",
                    "Advanced Programming Book",
                    2,
                    49.99m
                );
                Console.WriteLine("✓ Book seller notification sent!");
                
                Console.WriteLine("4. Testing Employee Notification...");
                emailService.SendEmployeeNotification(
                    "employee@example.com",
                    "employeeuser",
                    "ORD-12345",
                    "John Doe",
                    150,
                    15.00m
                );
                Console.WriteLine("✓ Employee notification sent!");
                
                Console.WriteLine("\n✅ All email tests completed successfully!");
                Console.WriteLine("\nNote: Emails are configured to use SMTP settings from Web.config");
                Console.WriteLine("Check your email configuration to ensure emails are actually delivered.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}