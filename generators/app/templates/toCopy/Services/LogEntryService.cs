using System;
using System.Linq;
using System.Linq.Expressions;
using Interfaces;
using Interfaces.Services;
using Models.Domain;

namespace Services
{
    public class LogEntryService : CrudService<LogEntry>, ILogEntryService
    {
        public LogEntryService(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        protected override Expression<Func<LogEntry, bool>> GetQueryPredicate(string searchTerm = "")
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return null;

            return (x => x.IdentityUserId.ToLower().Contains(searchTerm)
                || x.Level.ToLower().Contains(searchTerm)
                || x.Message.ToLower().Contains(searchTerm)
                || x.Category.ToLower().Contains(searchTerm)
                // For some reason we have to add the not null check so this doesn't throw 
                // a null reference exception
                || (x.UserName != null && x.UserName.ToLower().Contains(searchTerm)));
        }

        public void LogMessage(String logLevel, int eventId, String message, bool exception, string category, string identityUserId, string userName)
        {
            LogEntry entry = new LogEntry {
                Level = logLevel,
                EventId = eventId,
                Message = message,
                Exception = exception,
                Category = category,
                IdentityUserId = identityUserId,
                UserName = userName
            };

            try
            {
                this.Store(entry, "Logger");
            }
            catch (Exception)
            {
                // We're letting this fall through the cracks so we don't end up 
                // in an logging loop of death
            }
        }

        // Just hacking this in here to meke it easier to clear out the logs :)
        public void ClearLogs(string type)
        {
            if (string.IsNullOrWhiteSpace(type) || !type.Contains("remove-"))
                return;

            var logEntries = Models.AsEnumerable();

            if (type == "remove-all")
            {
                Models.RemoveRange(logEntries);
                DbContext.SaveChanges();
            }
            else if (type == "remove-old")
            {
                var oldestDate = DateTime.Now.AddDays(-30);
                logEntries = logEntries.Where(x => x.CreatedOn < oldestDate);
            }

            Models.RemoveRange(logEntries);
            DbContext.SaveChanges();
        }

        protected override LogEntry GetUpdatedOriginal(LogEntry original, LogEntry modified)
        {
            // This is all back-end stuff, so just pass it through
            original = modified;
            return original;
        }
    }
}
