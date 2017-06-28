using System.Threading.Tasks;

namespace Interfaces
{
    public interface INotificationService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
