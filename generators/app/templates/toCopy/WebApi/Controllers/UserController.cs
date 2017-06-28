using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppConfig;
using Interfaces;
using Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Domain;
using Models.Dto;
using WebApi.Models;
using WebApi.Models.AccountViewModels;
using WebApi.Models.ManageViewModels;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : BaseController<UserController> 
    {
        private readonly IEmailSender _emailSender;
        private readonly IApplicationUserService _applicationUserService;

        public UserController(
            IServiceProvider services,
            IEmailSender emailSender,
            IApplicationUserService applicationUserService)
            : base(services)
        {
            _emailSender = emailSender;
            _applicationUserService = applicationUserService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{pageNumber}/{pageSize}")]
        public PageResultViewModel<ApplicationUserSimpleDto> Get(
            uint pageNumber,
            uint pageSize,
            [FromQuery] string searchTerm = "",
            [FromQuery] string sortBy = "",
            [FromQuery] bool sortDesc = false)
        {
            Logger.LogInformation($"Get: pageNumber-{pageNumber}, pageSize-{pageSize}, searchTerm-{searchTerm}");

            var pageResults = new PageResultViewModel<ApplicationUserSimpleDto>
            {
                Results = _applicationUserService.Get(pageNumber, pageSize, searchTerm, sortBy, sortDesc),
                Total = _applicationUserService.Count(searchTerm),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return pageResults;
        }

        [Authorize(Roles="Admin")]
        [HttpGet("{id}")]
        public ApplicationUserSimpleDto Get(string id)
        {
            Logger.LogInformation($"Get: id-{id}");
            return _applicationUserService.Get(id);
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Update([FromBody] ApplicationUser model)
        {
            Logger.LogInformation($"Update: user-{model.UserName}, id-{model.Id}");

            var result = await _applicationUserService.Update(model);
            if (!result.Succeeded) {
                AddErrors(result);
                return BadRequest(ModelState);
            }
            return Ok(result);
        }
        
        [HttpGet("myAccount")]
        public ApplicationUserSimpleDto GetMyAccount()
        {
            Logger.LogInformation($"GetMyAccount");
            return CurrentUser;
        }

        [HttpGet("Roles")]
        [Authorize(Roles="Admin")]
        public IEnumerable<IdentityRole> GetRoles()
        {
            Logger.LogInformation($"GetRoles");
            return _applicationUserService.GetRoles();
        }

        [HttpGet("{id}/roles")]
        public Task<IList<string>> GetRoles(string id)
        {
            Logger.LogInformation($"GetRoles: id-{id}");
            var user = _applicationUserService.FindByIdAsync(id);
            return _applicationUserService.GetRolesAsync(user);
        }

        [HttpPost("{id}/roles")]
        public async Task<IdentityResult> UpdateRoles(string id, [FromBody] IList<string>roles)
        {
            Logger.LogInformation($"UpdateRoles: id-{id}, roles-{roles}");

            var user = _applicationUserService.FindByIdAsync(id);
            var allRoles = await _applicationUserService.GetRolesAsync(user);
            var removeResult = await _applicationUserService.RemoveFromRolesAsync(user, allRoles);

            return await _applicationUserService.AddToRolesAsync(user, roles);
        }

        //===========================================================
        // TODO: Everything below was copied from the AccountController & ManageController, 
        //       needs cleanup and use the ApplicationUserService
        //===========================================================
        [Authorize(Roles="Admin")]
        [HttpPost("register")]
        [Consumes("application/json")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            Logger.LogInformation($"Register: user-{model.Username}, email-{model.Email}");

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                var password = _applicationUserService.GeneratePassword();
                var userResult = await _applicationUserService.CreateAsync(user, password);

                if (!userResult.Succeeded) {
                    AddErrors(userResult);
                    return BadRequest(ModelState);
                }
                var roleResult = await _applicationUserService.AddToRolesAsync(user, model.Roles);
 
                if (!roleResult.Succeeded) {
                    AddErrors(roleResult);
                    return BadRequest(ModelState);
                }
                else
                {
                    var token = await _applicationUserService.GenerateEmailConfirmationTokenAsync(user);
                    var baseUrl = OptionsStore.ApplicationOptions.JwtOptions.Audience;
                    //            string baseUrl = _appOptions.Audience;
                    await _emailSender.SendEmailAsync(
                        model.Email, 
                        "You've been added as a user to Drive", 
                        $"Your username is <b>{model.Username}</b>.<br>" + 
                        "Please confirm your account and set your password on Drive by clicking " + 
                        $"<a href='{baseUrl}/account/confirm?token={token}'>this link</a>");
                }
            }

            return Ok();
        }
        
        [HttpPost("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromBody] ResetPasswordViewModel model)
        {
            Logger.LogInformation($"ConfirmEmail: user-{model.Username}");

            if (ModelState.IsValid) {
                
                var user = _applicationUserService.FindByNameAsync(model.Username);
                if (user == null)
                {
                    return BadRequest();
                }

                var confirmResult = await _applicationUserService.ConfirmEmailAsync(user, model.Code);
                if (!confirmResult.Succeeded)
                {
                    AddErrors(confirmResult);
                    return BadRequest(ModelState); 
                }

                var removeResult = await _applicationUserService.RemovePasswordAsync(user);
                if (!removeResult.Succeeded) {
                    AddErrors(removeResult);
                }

                var addResult = await _applicationUserService.AddPasswordAsync(user, model.Password);
                if (!addResult.Succeeded) {
                    AddErrors(addResult);
                }

                return Ok();
            }

            return BadRequest();
        }

        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            Logger.LogInformation($"ForgotPassword: email-{model.Email}");

            if (ModelState.IsValid)
            {
                var user = _applicationUserService.FindByEmailAsync(model.Email);
                if (user == null || !(await _applicationUserService.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return NotFound();
                }

                var token = await _applicationUserService.GeneratePasswordResetTokenAsync(user);
                var baseUrl = OptionsStore.ApplicationOptions.JwtOptions.Audience;

                await _emailSender.SendEmailAsync(
                    model.Email, 
                    "Drive Password Reset", 
                    $"Reset the password for <b>{user.UserName}</b> by clicking <a href='{baseUrl}/account/password/reset?token={token}'>this link</a>");

                return Ok();
            }

            return NotFound();
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            Logger.LogInformation($"ResetPassword: user-{model.Username}");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _applicationUserService.FindByNameAsync(model.Username);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return BadRequest();
            }

            var result = await _applicationUserService.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return Ok();
            }

            AddErrors(result);

            return BadRequest(ModelState);
        }
        
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            Logger.LogInformation($"ChangePassword");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = GetCurrentApplicationUser();
            if (user != null)
            {
                var result = await _applicationUserService.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    Logger.LogInformation($"User {user.FullName} changed their password successfully.");
                    return Ok(ManageMessageId.ChangePasswordSuccess);
                }
            }

            return BadRequest();
        }

        [HttpGet]
        public async Task<IEnumerable<ApplicationUser>> GetUsersInRole([FromQuery] string role)
        {
            Logger.LogInformation($"GetUsersInRole: role-{role}");
            return await _applicationUserService.GetUsersInRole(role);
        }

        [Authorize(Roles="Admin")]
        [HttpDelete("{id}")]
        public Task<IdentityResult> Delete(string id)
        {
            Logger.LogInformation($"Delete: id-{id}");
            var user = _applicationUserService.FindByIdAsync(id);
            return _applicationUserService.Delete(user);
        }

        [Authorize(Roles="Admin")]
        [HttpPost("{id}/RequirePasswordChange")]
        public async Task<IActionResult> RequirePasswordChange(string id)
        {
            Logger.LogInformation($"RequirePasswordChange: id-{id}");

            var user = _applicationUserService.FindByIdAsync(id);
            var removeResult = await _applicationUserService.RemovePasswordAsync(user);
            if (!removeResult.Succeeded) {
                AddErrors(removeResult);
                return BadRequest(ModelState);
            }
            var token = await _applicationUserService.GeneratePasswordResetTokenAsync(user);
            await _applicationUserService.SendPasswordResetEmailAsync(user.Email, user.UserName, token);
            return Ok();
        }
        
        private ApplicationUser GetCurrentApplicationUser()
        {
            Logger.LogInformation($"GetCurrentApplicationUser");
            return _applicationUserService.FindByNameAsync(CurrentUser.UserName);
        }
        
        public enum ManageMessageId
        {
            AddPhoneSuccess,
            AddLoginSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }
    }
}