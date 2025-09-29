using System;
using book2us.Services;
using book2us.Models;

namespace book2us.Tests
{
    public class EmailTest
    {
        public static void TestEmailService()
        {
            try
            {
                var emailService = new EmailService();
                
                // Test payment confirmation email
                emailService.SendPaymentConfirmation(
                    "test@example.com", 
                    "Test User", 
                    99.99m, 
                    "Book Purchase", 
                    "ORD-12345"
                );
                
                Console.WriteLine("Payment confirmation email sent successfully!");
                
                // Test printing status update email
                emailService.SendPrintingStatusUpdate(
                    "test@example.com",
                    "testuser",
                    123,
                    "Completed",
                    "Your printing request has been completed and is ready for pickup."
                );
                
                Console.WriteLine("Printing status update email sent successfully!");
                
                // Test book seller notification
                emailService.SendBookSellerNotification(
                    "seller@example.com",
                    "selleruser",
                    "ORD-12345",
                    "Test Book Title",
                    2,
                    49.99m
                );
                
                Console.WriteLine("Book seller notification email sent successfully!");
                
                // Test employee notification
                emailService.SendEmployeeNotification(
                    "employee@example.com",
                    "employeeuser",
                    "ORD-12345",
                    "Test Customer",
                    150,
                    15.00m
                );
                
                Console.WriteLine("Employee notification email sent successfully!");
                
                Console.WriteLine("All email tests completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}