using System.Net.Mail;
using System.Threading.Tasks;

namespace book2us.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            // In a real application, you would use a service like SendGrid or configure SMTP
            // For this example, we'll just write the email to the console
            System.Diagnostics.Debug.WriteLine("Sending email to " + email + " with subject \"" + subject + "\" and message: " + message);
            await Task.FromResult(0);
        }
    }
}