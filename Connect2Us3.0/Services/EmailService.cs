using System;
using System.Diagnostics;
using book2us.Models;

namespace book2us.Services
{
    public class EmailService
    {
        public void SendEmail(string to, string subject, string body)
        {
            // In a real application, this would integrate with SendGrid, SMTP, or another email service
            // For now, we'll just log the email to debug output
            Debug.WriteLine($"Email sent to: {to}");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"Body: {body}");
        }

        public void SendRegistrationConfirmation(string to, string userName)
        {
            var subject = "Welcome to Book2Us - Registration Confirmation";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #007bff;'>Welcome to Book2Us!</h2>
                    <p>Dear {userName},</p>
                    <p>Thank you for registering with Book2Us. Your account has been successfully created and you can now start using our services.</p>
                    <p>You can now:</p>
                    <ul>
                        <li>Browse and purchase books</li>
                        <li>Use our printing services</li>
                        <li>Manage your orders and wallet</li>
                    </ul>
                    <p>If you have any questions, please don't hesitate to contact our support team.</p>
                    <p>Best regards,<br>The Book2Us Team</p>
                    <hr>
                    <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
                </body>
                </html>
            ";
            SendEmail(to, subject, body);
        }

        public void SendPaymentConfirmation(string to, string userName, decimal amount, string orderType, string orderNumber)
        {
            var subject = $"Payment Confirmation - Order {orderNumber}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #28a745;'>Payment Confirmation</h2>
                    <p>Dear {userName},</p>
                    <p>Your payment has been successfully processed.</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h3>Payment Details:</h3>
                        <p><strong>Order Number:</strong> {orderNumber}</p>
                        <p><strong>Order Type:</strong> {orderType}</p>
                        <p><strong>Amount Paid:</strong> ${amount:F2}</p>
                        <p><strong>Payment Date:</strong> {DateTime.Now:MMMM dd, yyyy}</p>
                    </div>
                    <p>Thank you for your business!</p>
                    <p>Best regards,<br>The Book2Us Team</p>
                    <hr>
                    <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
                </body>
                </html>
            ";
            SendEmail(to, subject, body);
        }

        public void SendPrintingStatusUpdate(string to, string userName, int requestId, string status, string additionalInfo = "")
        {
            var subject = $"Printing Status Update - Request #{requestId}";
            string statusColor;
            switch (status.ToLower())
            {
                case "completed":
                    statusColor = "#28a745";
                    break;
                case "processing":
                    statusColor = "#ffc107";
                    break;
                case "cancelled":
                    statusColor = "#dc3545";
                    break;
                default:
                    statusColor = "#007bff";
                    break;
            }

            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: {statusColor};'>Printing Status Update</h2>
                    <p>Dear {userName},</p>
                    <p>Your printing request status has been updated.</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h3>Request Details:</h3>
                        <p><strong>Request ID:</strong> #{requestId}</p>
                        <p><strong>Current Status:</strong> <span style='color: {statusColor}; font-weight: bold;'>{status}</span></p>
                        <p><strong>Update Date:</strong> {DateTime.Now:MMMM dd, yyyy HH:mm}</p>
                        {(string.IsNullOrEmpty(additionalInfo) ? "" : $"<p><strong>Additional Information:</strong> {additionalInfo}</p>")}
                    </div>
                    <p>Best regards,<br>The Book2Us Team</p>
                    <hr>
                    <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
                </body>
                </html>
            ";
            SendEmail(to, subject, body);
        }

        public void SendBookSellerNotification(string to, string sellerName, string orderType, int orderId, decimal amount, string customerName)
        {
            var subject = $"New {orderType} Order - #{orderId}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #007bff;'>New Order Notification</h2>
                    <p>Dear {sellerName},</p>
                    <p>You have received a new {orderType.ToLower()} order.</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h3>Order Details:</h3>
                        <p><strong>Order Number:</strong> #{orderId}</p>
                        <p><strong>Order Type:</strong> {orderType}</p>
                        <p><strong>Customer:</strong> {customerName}</p>
                        <p><strong>Order Amount:</strong> ${amount:F2}</p>
                        <p><strong>Order Date:</strong> {DateTime.Now:MMMM dd, yyyy HH:mm}</p>
                    </div>
                    <p>Please log in to your seller dashboard to view and process this order.</p>
                    <p>Best regards,<br>The Book2Us Team</p>
                    <hr>
                    <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
                </body>
                </html>
            ";
            SendEmail(to, subject, body);
        }

        public void SendEmployeeNotification(string to, string employeeName, int requestId, string customerName, string serviceType)
        {
            var subject = $"New {serviceType} Request - #{requestId}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #007bff;'>New Work Assignment</h2>
                    <p>Dear {employeeName},</p>
                    <p>A new {serviceType.ToLower()} request has been assigned to the team.</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h3>Request Details:</h3>
                        <p><strong>Request ID:</strong> #{requestId}</p>
                        <p><strong>Service Type:</strong> {serviceType}</p>
                        <p><strong>Customer:</strong> {customerName}</p>
                        <p><strong>Request Date:</strong> {DateTime.Now:MMMM dd, yyyy HH:mm}</p>
                    </div>
                    <p>Please log in to your employee dashboard to review and process this request.</p>
                    <p>Best regards,<br>The Book2Us Team</p>
                    <hr>
                    <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
                </body>
                </html>
            ";
            SendEmail(to, subject, body);
        }

        public void SendPasswordResetLink(string to, string userName, string resetToken)
        {
            var subject = "Password Reset Request - Book2Us";
            var resetUrl = $"https://localhost:44300/Account/ResetPassword?token={resetToken}";
            
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #dc3545;'>Password Reset Request</h2>
                    <p>Dear {userName},</p>
                    <p>We received a request to reset your password for your Book2Us account.</p>
                    <p>Please click the button below to create a new password:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetUrl}' style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Reset Password</a>
                    </div>
                    <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                    <p style='word-break: break-all; color: #007bff;'>{resetUrl}</p>
                    <p><strong>This link will expire in 24 hours for security reasons.</strong></p>
                    <p>If you didn't request this password reset, please ignore this email. Your account will remain secure.</p>
                    <p>Best regards,<br>The Book2Us Team</p>
                    <hr>
                    <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
                </body>
                </html>
            ";
            SendEmail(to, subject, body);
        }
    }
}