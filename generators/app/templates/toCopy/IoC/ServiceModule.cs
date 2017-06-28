using Interfaces;
using Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Services;

namespace IoC
{
    public static class ServiceModule
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            // Transient: A new instance of the type is used every time the type is requested.
            // Scoped:    A new instance of the type is created the first time it’s requested within a 
            //            given HTTP request, and then re - used for all subsequent types resolved during that HTTP request.
            // Singleton: A single instance of the type is created once, and used by all subsequent requests for that type.

            // Add application services
            // Root
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IApplicationUserService, ApplicationUserService>();
            services.AddScoped<ILogEntryService, LogEntryService>();

            return services;
        }
    }
}
