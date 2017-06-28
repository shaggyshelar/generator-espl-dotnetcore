using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using AppConfig;
using Interfaces;
using Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Models.Domain;
using Models.Dto;

namespace Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IEmailSender _emailSender;
        private readonly IApplicationUserMap _map;
        private readonly ApplicationOptions _appOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationUserService(
            IEmailSender emailSender,
            IApplicationUserMap map,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _emailSender = emailSender;
            _map = map;
            _appOptions = OptionsStore.ApplicationOptions;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // Returns a simple dto
        public ApplicationUserSimpleDto GetByUserName(string userName)
        {
            var task = _userManager.FindByNameAsync(userName);
            task.Wait();
            var applicationUser = task.Result;
            return _map.ConvertToSimple(applicationUser);
        }

        // Returns a simple dto
        public ApplicationUserSimpleDto GetByEmail(string email)
        {
            var task = _userManager.FindByEmailAsync(email);
            task.Wait();
            var applicationUser = task.Result;
            return _map.ConvertToSimple(applicationUser);
        }
        
        public IEnumerable<ApplicationUserSimpleDto> Get(
            uint pageNumber,
            uint pageSize,
            string searchTerm = "",
            string sortBy = "",
            bool sortDesc = false)
        {
            IQueryable<ApplicationUser> users = _userManager.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();

                var predicate = GetQueryPredicate(searchTerm);
                users = users.Where(predicate);
            }

  			 if (string.IsNullOrWhiteSpace(sortBy))
            {
                // Default sort is edited descending
                users = users
                    .OrderByDescending(x => x.LastName).ThenByDescending(x => x.FirstName);
            }
            else
            {
                // Otherwise, dynamically sort by
                var sortType = sortDesc ? "DESC" : "ASC";
                var dynamicSort = $"{sortBy} {sortType}";

                users = users.OrderBy(dynamicSort);

            }
            users = users
                .Skip(Convert.ToInt32(pageSize) * Convert.ToInt32(pageNumber)).Take(Convert.ToInt32(pageSize));

            return _map.ConvertToSimple(users);
        }

		private Expression<Func<ApplicationUser, bool>> GetQueryPredicate(string searchTerm = "")
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return null;

			return (x => x.Email.ToLower().Contains(searchTerm)
                            || x.FirstName.ToLower().Contains(searchTerm)
                            || x.LastName.ToLower().Contains(searchTerm)
                            || x.UserName.ToLower().Contains(searchTerm)
                            || x.Id.ToLower().Contains(searchTerm));
		}
	 

        // Returns a simple dto
        public ApplicationUserSimpleDto Get(string id)
        {
            var user = _userManager.Users.Where(x => x.Id == id).FirstOrDefault();
            return _map.ConvertToSimple(user);
        }

        public int Count(string searchTerm = "")
        {
 			var users = _userManager.Users;
			if (!string.IsNullOrWhiteSpace(searchTerm)) {
				var predicate = GetQueryPredicate(searchTerm);
				users = users.Where(predicate);
			}

			return users.Count();
        }

        public IEnumerable<IdentityRole> GetRoles()
        {
            return _roleManager.Roles.ToList();
        }

        public Task<IdentityResult> Update(ApplicationUser model)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == model.Id);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.UserName = model.UserName;

            return _userManager.UpdateAsync(user);
        }

        public Task<IdentityResult> Delete(ApplicationUser model)
        {
            return _userManager.DeleteAsync(model);
        }

        //===========================================================================
        // NOTE: Everything below here was added just to satify the methods on the 
        //       UserController that were combined from the scaffolded Account and
        //       Manage controllers.
        //       I'm just dropping them in here quickly to get stuff working.
        //       This should be cleaned up a little at some point. :) ~ Nicholas
        //===========================================================================

        // UserManager methods
        public Task<IdentityResult> CreateAsync(ApplicationUser model, string password)
        {
            return _userManager.CreateAsync(model, password);
        }

        public Task<IdentityResult> AddToRolesAsync(ApplicationUser model, IEnumerable<string> roles)
        {
            return _userManager.AddToRolesAsync(model, roles);
        }

        public Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser model, IEnumerable<string> roles)
        {
            return _userManager.RemoveFromRolesAsync(model, roles);
        }

        public Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser model)
        {
            return _userManager.GenerateEmailConfirmationTokenAsync(model);
        }

        public string GeneratePassword()
        {
            string digitChars = "0123456789";
            string upperChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            string lowerChars = "abcdefghijkmnopqrstuvwxyz";
            string specialChars = "!@#$%^&*()";
            string allowedChars = digitChars + upperChars + lowerChars + specialChars;
            Random random = new Random();
            char[] chars = new char[10];

            chars[0] = upperChars[(int)((upperChars.Length) * random.NextDouble())];
            for (int i = 1; i < 7; i++)
            {
                chars[i] = allowedChars[(int)((allowedChars.Length) * random.NextDouble())];
            }
            chars[7] = digitChars[(int)((digitChars.Length) * random.NextDouble())];
            chars[8] = specialChars[(int)((specialChars.Length) * random.NextDouble())];
            chars[9] = lowerChars[(int)((lowerChars.Length) * random.NextDouble())];

            return new string(chars);
        }

        public ApplicationUser FindByIdAsync(string id)
        {
            var task = _userManager.FindByIdAsync(id);
            task.Wait();
            return task.Result;
        }

        public ApplicationUser FindByNameAsync(string userName)
        {
            var task = _userManager.FindByNameAsync(userName);
            task.Wait();
            return task.Result;
        }

        public ApplicationUser FindByEmailAsync(string email)
        {
            var task = _userManager.FindByEmailAsync(email);
            task.Wait();
            return task.Result;
        }

        public Task SendRegisterEmail(string email, string username, string token)
        {
            string baseUrl = _appOptions.JwtOptions.Audience;
            return _emailSender.SendEmailAsync(email, "You've been added as a user to Drive", "Your username is <b>" + username + "</b>.<br>Please confirm your account and set your password on Drive by clicking <a href='" + baseUrl + "/account/confirm?token=" + token + "'>this link</a>");
        }

        public Task SendPasswordResetEmailAsync(string email, string username, string token)
        {
            string baseUrl = _appOptions.JwtOptions.Audience;
            return _emailSender.SendEmailAsync(email, "Drive Password Reset", "Reset password for <b>" + username + "</b> by clicking <a href='" + baseUrl + "/account/password/reset?token=" + token + "'>this link</a>");
        }

        public Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token)
        {
            return _userManager.ConfirmEmailAsync(user, token);
        }

        public Task<bool> IsEmailConfirmedAsync(ApplicationUser user)
        {
            return _userManager.IsEmailConfirmedAsync(user);
        }

        public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            return _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
        {
            return _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public Task<ApplicationUser> GetUserAsync(ClaimsPrincipal principal)
        {
            return _userManager.GetUserAsync(principal);
        }

        public Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return _userManager.CheckPasswordAsync(user, password);
        }

        public Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            return _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public Task<IdentityResult> AddPasswordAsync(ApplicationUser user, string password)
        {
            return _userManager.AddPasswordAsync(user, password);
        }

        public Task<IdentityResult> RemovePasswordAsync(ApplicationUser user)
        {
            return _userManager.RemovePasswordAsync(user);
        }

        public Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return _userManager.GetRolesAsync(user);
        }

        // SignIn Manager methods
        public Task SignInAsync(ApplicationUser user, bool isPersistent, string authenticationMethod = null)
        {
            return _signInManager.SignInAsync(user, isPersistent, authenticationMethod);
        }

        public Task SignOutAsync(string id)
        {
            // Padding through the User.Id just in case we want to do something with it later on
            return _signInManager.SignOutAsync();
        }

        public Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            return _signInManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
        }

        public Task<IList<ApplicationUser>> GetUsersInRole(string role)
        {
            return _userManager.GetUsersInRoleAsync(role);
        }
    }
}