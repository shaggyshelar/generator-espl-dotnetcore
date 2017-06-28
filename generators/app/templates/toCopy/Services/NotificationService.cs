using System.Threading.Tasks;
using Interfaces;
using MailKit;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;
using AppConfig;

namespace Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class NotificationService : INotificationService
    {

        private readonly EmailerOptions emailerOptions;

        public NotificationService() {
            this.emailerOptions = OptionsStore.ApplicationOptions.EmailerOptions;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            // Plug in your email service here to send an email.
            var emailMessage = new MimeMessage();
            
                emailMessage.From.Add(new MailboxAddress("TODO", "TODO"));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("html") { Text = message };
            
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(emailerOptions.Server, 25, SecureSocketOptions.None).ConfigureAwait(false);
                    await client.AuthenticateAsync(emailerOptions.Username, emailerOptions.Password);
                    await client.SendAsync(emailMessage).ConfigureAwait(false);
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
        }
    }
}
