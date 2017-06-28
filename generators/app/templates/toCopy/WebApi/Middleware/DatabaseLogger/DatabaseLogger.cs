using System;
using System.Linq;
using Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models.Domain;
using Newtonsoft.Json;
using WebApi.Providers;

namespace WebApi.Middleware.Logging
{
    public class DatabaseLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ILogEntryService _logEntries;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IApplicationUserService _applicationUsers;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FilterLoggerSettings _filterSettings;
   
        public DatabaseLogger(
            string categoryName, 
            ILogEntryService logEntries,
            ICurrentUserProvider currentUserProvider,
            IApplicationUserService applicationUsers,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            FilterLoggerSettings filterSettings)
        {
            _categoryName = categoryName;
            _logEntries = logEntries;
            _currentUserProvider = currentUserProvider;
            _applicationUsers = applicationUsers;
            _userManager = userManager;
            _filterSettings = filterSettings;
            _httpContextAccessor = httpContextAccessor;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NoopDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // Find a category match in the filter switches
            var match = _filterSettings.Switches.FirstOrDefault(x => _categoryName.Contains(x.Key));
            if (match.Key == null)
            {
                // There is no explicit filter for this
                // So should we filter it out, or let it through?
                //      Filtering it out for now
                return false;
            }
            else
            {
                // There's an explicit filter level setting
                // The passed in log level needs to be equal to, or higher than the match level
                return logLevel >= match.Value;
            }
        }

        public void Log<TState>(
            LogLevel logLevel, 
            EventId eventId, 
            TState state, 
            Exception exception, 
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));
            
            try
            {
                var message = formatter(state, exception);

                if (string.IsNullOrWhiteSpace(message))
                    return;

                if (exception != null)
                    message += Environment.NewLine + Environment.NewLine + exception.ToString();

                var userName = (_httpContextAccessor.HttpContext == null)
                    ? null
                    : _httpContextAccessor.HttpContext.User?.Identity?.Name;

                var applicationUser = new ApplicationUser();

                if (!string.IsNullOrWhiteSpace(userName))
                {
                    var task = _userManager.FindByNameAsync(userName);
                    task.Wait();
                    applicationUser = task.Result;
                }

                // Trying to make sure the userName is never null/empty
                userName = string.IsNullOrWhiteSpace(applicationUser.UserName)
                    ? "N/A"
                    : applicationUser.UserName;

                if (_httpContextAccessor.HttpContext != null && userName == "N/A")
                {

                    var test = _currentUserProvider.GetUser();

                    _logEntries.LogMessage(
                        logLevel.ToString(),
                        eventId.Id,
                        message + " - CurrentUser not found! - " + JsonConvert.SerializeObject(_httpContextAccessor.HttpContext.User),
                        exception != null,
                        _categoryName,
                        applicationUser.Id,
                        userName);
                }

                _logEntries.LogMessage(
                    logLevel.ToString(),
                    eventId.Id,
                    message,
                    exception != null,
                    _categoryName,
                    applicationUser.Id,
                    userName);
            }
            catch (Exception ex)
            {
                _logEntries.LogMessage(
                    LogLevel.Error.ToString(),
                    eventId.Id,
                    "Exception during log: " + ex.Message,
                    true,
                    _categoryName,
                    "N/A",
                    "N/A");
            }
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
