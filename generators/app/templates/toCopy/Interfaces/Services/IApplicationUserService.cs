using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Models.Domain;
using Models.Dto;

namespace Interfaces.Services
{
    public interface IApplicationUserService 
    {
        IEnumerable<ApplicationUserSimpleDto> Get(
            uint pageNumber,
            uint pageSize,
            string searchTerm = "",
            string sortBy = "",
            bool sortDesc = false);
        ApplicationUserSimpleDto Get(string id);
        int Count(string searchTerm = "");
        IEnumerable<IdentityRole> GetRoles();
        Task<IdentityResult> Update(ApplicationUser user);
        Task<IdentityResult> Delete(ApplicationUser user);
        ApplicationUserSimpleDto GetByUserName(string userName);
        ApplicationUserSimpleDto GetByEmail(string email);
        Task SendRegisterEmail(string email, string username, string token);
        Task SendPasswordResetEmailAsync(string email, string username, string token);
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
        string GeneratePassword();
        // User manager methods
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles);
        Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles);
        ApplicationUser FindByIdAsync(string userId);
        ApplicationUser FindByNameAsync(string userName);
        ApplicationUser FindByEmailAsync(string email);
        Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);
        Task<bool> IsEmailConfirmedAsync(ApplicationUser user);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
        Task<ApplicationUser> GetUserAsync(ClaimsPrincipal principal);
        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
        Task<IdentityResult> AddPasswordAsync(ApplicationUser user, string password);
        Task<IdentityResult> RemovePasswordAsync(ApplicationUser user);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);

        // Signin manager methods
        Task SignInAsync(ApplicationUser user, bool isPersistent, string authenticationMethod = null);
        Task SignOutAsync(string userName);
        Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure);
        Task<IList<ApplicationUser>> GetUsersInRole(string role);
    }
}