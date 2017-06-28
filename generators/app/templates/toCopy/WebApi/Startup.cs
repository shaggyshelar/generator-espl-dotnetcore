using System;
using System.IO;
using System.Text;
using AppConfig;
using Interfaces.Services;
using IoC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Models.Domain;
using WebApi.Middleware.Logging;
using WebApi.Providers;

namespace WebApi
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private bool _noSecrets = false;

        public Startup(IHostingEnvironment env)
        {
            _env = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(_env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName.ToLower()}.json", optional: true);

            if (_env.IsDevelopment() || _env.IsStaging())
            {
                // This will push telemetry data through Application Insights pipeline 
                // faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);

                try
                {
                    // We'll use user secrets where able
                    // This will throw an error on Azure
                    builder.AddUserSecrets();
                }
                catch
                {
                    // If we landed here, we're on Azure and don't have user secrets
                    _noSecrets = true;
                }
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // THIS DOESN'T WORK, the objects are all null >:(
            //      Probably have to set up the config sections to match the objects, screw it
            // Allows us to inject options/configuration/appsettings.json values with DI
            //services.AddOptions();
            //services.Configure<ApplicationOptions>(options => options = appOptions);
            //services.Configure<TokenProviderOptions>(options => options = GetTokenProviderOptions(appOptions));

            // HACK! Because AddOptions is a pile of crap :)
            OptionsStore.SetApplicationOptions(GetApplicationOptions());
            OptionsStore.SetTokenProviderOptions(GetTokenProviderOptions());

            // Cross origin middleware
            services.AddCors(options =>
            {
                options.AddPolicy("MyCorsPolicy", builder =>
                    builder.AllowCredentials()
                        .WithOrigins(
                            //OptionsStore.ApplicationOptions.JwtOptions.Issuer,
                            OptionsStore.ApplicationOptions.JwtOptions.Audience)
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            #region IoC Extensions

            services.AddDataContext(OptionsStore.ApplicationOptions.DbConnection);
            services.AddDomainServices();
            services.AddDomainServiceMapping();
            services.AddSeedServices();
            services.AddDomainWorkflows();

            #endregion

            // Add the WebAPI providers
            services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
            services.AddScoped<ITokenProvider, TokenProvider>();

            // Add framework services
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddMvc();
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("MyCorsPolicy"));
            });

            // This is used to stop the API from trying to redirect to a route on unauthorized attempts
            services.Configure<IdentityOptions>(options =>
            {
                options.Cookies.ApplicationCookie.AutomaticAuthenticate = false;
                options.Cookies.ApplicationCookie.AutomaticChallenge = false;
                options.Cookies.ApplicationCookie.LoginPath = PathString.Empty;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            });

            #region swagger stuff

            string basePath = PlatformServices.Default.Application.ApplicationBasePath;
            string pathToDoc = basePath + Path.DirectorySeparatorChar + "WebApi.xml";
            
            services.AddSwaggerGen();

            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Swashbuckle.Swagger.Model.Info
                {
                    Version = "v1",
                    Title = ".Net Core WebAPI",
                    Description = "An API API With Swagger",
                    TermsOfService = "None"
                });
                options.IncludeXmlComments(pathToDoc);
                options.DescribeAllEnumsAsStrings();
            });

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline  
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            ICurrentUserProvider currentUserProvider,
            IApplicationUserService applicationUsers,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            ILogEntryService logEntries,
            ISeedService seedService)
        {
            app.UseCors("MyCorsPolicy");

            // This is necessary to use the Identity services
            app.UseIdentity();

            // Application insights stuff
            app.UseApplicationInsightsRequestTelemetry();
            app.UseApplicationInsightsExceptionTelemetry();

            // This allows us to log in to swagger
            app.UseStaticFiles();

            // Do environment specific things
            if (!_env.IsProduction())
            {
                // Do non-production environment things

                // This will show detailed error information as long as the environment is "Development"
                // NOTE: The Azure App setting needs to be added as well for this to work
                app.UseDeveloperExceptionPage();

                // Debug console logging
                loggerFactory
                    .WithFilter(new FilterLoggerSettings {
                        { "Microsoft", LogLevel.Warning },
                        { "System", LogLevel.Warning },
                        { "WebApi", LogLevel.Debug }
                    })
                    .AddConsole();

                // Seed some test data
                seedService.Run();
            }
            else
            {
                // Do production environment things

            }

            // Set up the database logging middleware
            loggerFactory.AddDatabase(currentUserProvider, applicationUsers, userManager, logEntries, httpContextAccessor, GetLoggerFilterSettings());

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AuthenticationScheme = "JWT Bearer Token",
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                RequireHttpsMetadata = false,
                TokenValidationParameters = GetTokenValidationParameters(),
            });

            // This has to happen before we add Swashbuckle/Swagger
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi();
        }

        private FilterLoggerSettings GetLoggerFilterSettings()
        {
            // Persist logging to the database
            // The filter will match on 'contains', so you can put as much or as little of the 
            // namespace in as you like. Anything not listed in the filter gets filtered out by default.

            var settings = new FilterLoggerSettings();

            if (!_env.IsProduction())
            {
                // Everything but the production environment

                settings = new FilterLoggerSettings {
                    { "Microsoft", LogLevel.Error },
                    { "System", LogLevel.Warning },
                    { "WebApi", LogLevel.Debug },
                    { "Data", LogLevel.Debug },
                    { "IoC", LogLevel.Debug },
                    { "Interfaces", LogLevel.Debug },
                    { "Models", LogLevel.Debug },
                    { "Services", LogLevel.Debug },
                    { "Workflows", LogLevel.Debug }
                };
            }
            else
            {
                // Production environment

                settings = new FilterLoggerSettings {
                    { "Microsoft", LogLevel.Error },
                    { "System", LogLevel.Error },
                    { "WebApi", LogLevel.Warning },
                    { "Data", LogLevel.Warning },
                    { "IoC", LogLevel.Warning },
                    { "Interfaces", LogLevel.Warning },
                    { "Models", LogLevel.Warning },
                    { "Services", LogLevel.Warning },
                    { "Workflows", LogLevel.Warning }
                };
            }

            return settings;
        }

        private ApplicationOptions GetApplicationOptions()
        {
            // NOTE: We're not caching the output of this anywhere just in case the json changes
            //       BUT: We are dumping it into a static class so the DbContextFactory can get to it, though
            //       I'm not seeing where that code is actually getting hit. It's a work-around for now anyway. /shrug

            // We're always going to pull the Env from appsettings.json to use for debugging
            var applicationOptions = new ApplicationOptions
            {
                Environment = Configuration["ApplicationOptions:Environment"]
            };

            applicationOptions.DbConnection = (_noSecrets)
                // We're on Azure, pull from appsettings.json
                ? Configuration["ApplicationOptions:DbConnection"]
                // We're in a local development environment that should have user secrets set up
                : Configuration["UserSecrets:DbConnection"];

            // Throw an exception if we're missing the connection string 
            if (applicationOptions.DbConnection == null)
            {
                // If you hit this exception, you might need to add user secrets for the DB connection
                // Set name: "UserSecrets:DbConnection"
                // Example connection string: "Server=(localdb)\\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;MultipleActiveResultSets=true"
                throw new Exception("Startup.GetApplicationOptions: No DB connection was found in environment settings! " +
                    "(Env:" + _env.EnvironmentName + ", AppSettings Env: " + Configuration["ApplicationOptions:Environment"] + ")");
            }

            applicationOptions.EmailerOptions = new EmailerOptions
            {
                Server = _noSecrets 
                    ? Configuration["ApplicationOptions:Emailer:Server"] 
                    : Configuration["UserSecrets:Emailer:Server"],
                Username = _noSecrets 
                    ? Configuration["ApplicationOptions:Emailer:Username"] 
                    : Configuration["UserSecrets:Emailer:Username"],
                Password = _noSecrets 
                    ? Configuration["ApplicationOptions:Emailer:Password"] 
                    : Configuration["UserSecrets:Emailer:Password"]
            };

            if (applicationOptions.EmailerOptions.Server == null 
                || applicationOptions.EmailerOptions.Username == null 
                || applicationOptions.EmailerOptions.Password == null)
            {
                throw new Exception("Startup.GetApplicationOptions: Missing email environment settings! " +
                    $"(Env: {_env.EnvironmentName}, AppSettings Env: {Configuration["ApplicationOptions:Environment"]})");
            }

            applicationOptions.JwtOptions = new JwtOptions {
                Issuer = _noSecrets
                    ? Configuration["ApplicationOptions:Jwt:Issuer"]
                    : Configuration["UserSecrets:Jwt:Issuer"],
                Audience = _noSecrets
                    ? Configuration["ApplicationOptions:Jwt:Audience"]
                    : Configuration["UserSecrets:Jwt:Audience"],
                SecretPassword = Configuration["ApplicationOptions:Jwt:SecretPassword"]
            };

            if (applicationOptions.JwtOptions.Issuer == null
                || applicationOptions.JwtOptions.Audience == null
                || applicationOptions.JwtOptions.SecretPassword == null)
            {
                throw new Exception("Startup.GetApplicationOptions: missing JWT environment settings! " +
                    $"(Env: {_env.EnvironmentName}, AppSettings Env: {Configuration["ApplicationOptions:Environment"]})");
            }

            return applicationOptions;
        }

        private TokenProviderOptions GetTokenProviderOptions()
        {
            // The signing key will always be pulled from appsettings.json
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(OptionsStore.ApplicationOptions.JwtOptions.SecretPassword));

            // Options will be pulled from either appsettings.json, or user secrets, depending on the env.
            var options = new TokenProviderOptions
            {
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            };

            options.Issuer = OptionsStore.ApplicationOptions.JwtOptions.Issuer;
            options.Audience = OptionsStore.ApplicationOptions.JwtOptions.Audience;

            // Throw an error if we don't have required values 
            if (options.Issuer == null || options.Audience == null)
            {
                // If you hit this exception, you might need to add user secrets for JWT settings
                // Set names: "UserSecrets:Jwt:Audience" and "UserSecrets:Jwt:Issuer"
                // Reference: https://docs.asp.net/en/latest/security/app-secrets.html
                // Examples: 
                //      "UserSecrets:Jwt:Issuer" : "http://localhost:5000"
                //      "UserSecrets:Jwt:Audience" : "http://localhost:4200"
                throw new Exception("Startup.GetTokenProviderOptions: Missing JWT environment settings! " +
                    "(Env:" + _env.EnvironmentName + ", " + Configuration["ApplicationOptions:Environment"] + ")");
            }

            return options;
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            // secretKey contains a secret passphrase only your server knows
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(OptionsStore.ApplicationOptions.JwtOptions.SecretPassword));

            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = OptionsStore.ApplicationOptions.JwtOptions.Issuer,

                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = OptionsStore.ApplicationOptions.JwtOptions.Audience,

                // Validate the token expiry
                ValidateLifetime = true,

                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.Zero
            };

            return tokenValidationParameters;
        }
    }
}
