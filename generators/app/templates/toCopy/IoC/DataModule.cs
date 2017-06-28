using Data;
using Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Models.Domain;

namespace IoC
{
    public static class DataModule
    {
        public static IServiceCollection AddDataContext(this IServiceCollection services, string connectionString)
        {
            // Transient: A new instance of the type is used every time the type is requested.
            // Scoped:    A new instance of the type is created the first time it’s requested within a 
            //            given HTTP request, and then re - used for all subsequent types resolved during that HTTP request.
            // Singleton: A single instance of the type is created once, and used by all subsequent requests for that type.

            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

            services.AddDbContext<ApplicationDbContext>(options =>
                   options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}
