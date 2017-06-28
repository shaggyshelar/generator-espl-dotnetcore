using System;
using System.Threading.Tasks;
using Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApi.Models.AccountViewModels;
using WebApi.Providers;

namespace WebApi.Controllers
{
    [Route("api")]
    public class LoginController : BaseController<LoginController> 
    {
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ITokenProvider _tokenProvider;

        public LoginController(
            IServiceProvider services,
            IApplicationUserService applicationUserService,
            ITokenProvider tokenProvider)
            : base(services)
        {
            _serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            _applicationUserService = applicationUserService;
            _tokenProvider = tokenProvider;
        }
        
        [HttpPost("logoff")]
        public async Task LogOff()
        {
            Logger.LogInformation($"LogOff");
            await _applicationUserService.SignOutAsync(CurrentUser.UserName);
        }
        
        [AllowAnonymous]
        [HttpPost("token")]
        public async Task Token([FromBody] LoginViewModel model)
        {
            Logger.LogInformation($"Token: user-{model.UserName}");

            var token = _tokenProvider.GenerateToken(model.UserName, model.Password);

            if (token == null)
            {
                // Invalid login credentials
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsync("Invalid username or password.");
            }
            else
            {
                // User was logged in successfully
                HttpContext.Response.ContentType = "application/json";
                await HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(token, _serializerSettings));
            }

            return;
        }
    }
}