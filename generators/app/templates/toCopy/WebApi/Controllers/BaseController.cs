using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models.Dto;
using WebApi.Providers;

namespace WebApi.Controllers
{
    public abstract class BaseController<TController> : Controller
    {
        public BaseController(IServiceProvider services)
        {
            var currentUserProvider = services.GetService<ICurrentUserProvider>();
            CurrentUser = currentUserProvider.GetUser();

            var loggerFactory = services.GetService<ILoggerFactory>();
            Logger = loggerFactory.CreateLogger<TController>();
        }

        protected readonly ILogger Logger;

        protected ApplicationUserSimpleDto CurrentUser { get; private set; }

        protected void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}