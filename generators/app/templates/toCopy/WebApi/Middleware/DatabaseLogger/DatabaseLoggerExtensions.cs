using Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models.Domain;
using WebApi.Providers;

namespace WebApi.Middleware.Logging
{
    public static class DatabaseLoggerExtentions
    {
        public static ILoggerFactory AddDatabase(
            this ILoggerFactory factory,
            ICurrentUserProvider currentUserProvider,
            IApplicationUserService applicationUsers,
            UserManager<ApplicationUser> userManager, 
            ILogEntryService logEntries,
            IHttpContextAccessor httpContextAccessor,
            FilterLoggerSettings filterSettings)
        {
            factory.AddProvider(new DatabaseLoggerProvider(currentUserProvider, logEntries, applicationUsers, httpContextAccessor, userManager, filterSettings));
            return factory;
        }
    }
}
