using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Models.Domain;
using Models.Dto;

namespace WebApi.Providers
{
    public interface ICurrentUserProvider
    {
        ApplicationUserSimpleDto GetUser();
    }

    public class CurrentUserProvider : ICurrentUserProvider
    {
        private readonly HttpContext _httpContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public CurrentUserProvider(
            IHttpContextAccessor context,
            UserManager<ApplicationUser> userManager)
        {
            _httpContext = context.HttpContext;
            _userManager = userManager;
        }

        public ApplicationUserSimpleDto GetUser()
        {
            if (_httpContext == null || _userManager == null)
                return null;

            var userName = _httpContext.User?.Identity?.Name;
            if (userName != null)
            {
                var task = _userManager.FindByNameAsync(userName);
                task.Wait();

                if (task.Result != null)
                {
                    return new ApplicationUserSimpleDto
                    {
                        Id = task.Result.Id,
                        FirstName = task.Result.FirstName,
                        LastName = task.Result.LastName,
                        Email = task.Result.Email,
                        UserName = task.Result.UserName
                    };
                }
            }
            return null;
        }
    }
}
