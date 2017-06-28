using Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models.Domain;
using WebApi.Providers;

namespace WebApi.Middleware.Logging
{
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly FilterLoggerSettings _filterSettings;
        private readonly ILogEntryService _logEntries;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IApplicationUserService _applicationUsers;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public DatabaseLoggerProvider(
            ICurrentUserProvider currentUserProvider, 
            ILogEntryService logEntries,
            IApplicationUserService applicationUsers,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            FilterLoggerSettings filterSettings) 
        {
            _currentUserProvider = currentUserProvider;
            _logEntries = logEntries;
            _applicationUsers = applicationUsers;
            _userManager = userManager;
            _filterSettings = filterSettings;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public ILogger CreateLogger(string categoryName)
        {
            return new DatabaseLogger(categoryName, _logEntries, _currentUserProvider, 
                _applicationUsers, _httpContextAccessor, _userManager, _filterSettings);
        }

        public void Dispose()
        {
        }
    }
}
