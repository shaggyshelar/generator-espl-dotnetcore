using Models.Domain;

namespace Interfaces.Services
{
    public interface ILogEntryService : ICrudService<LogEntry>
    {
        void LogMessage(string logLevel, int eventId, string message, bool exception, string category, string identityUserId, string userName);
        void ClearLogs(string type);
    }
}
