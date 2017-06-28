using System.Collections.Generic;
using Interfaces.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Domain;

namespace Data.Seeds
{
    public class ApplicationUserSeed : BaseSeed<ApplicationUserSeed>, IApplicationUserSeed
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationUserSeed(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Process()
        {
            SeedRoles();
            SeedUsers();
        }

        private void SeedRoles()
        {
            var roles = new List<IdentityRole>();
            roles.Add(new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" });
            roles.Add(new IdentityRole { Name = "Developer", NormalizedName = "DEVELOPER" });

            foreach (var role in roles)
            {
                var task = _roleManager.CreateAsync(role);
                task.Wait();

                if (task.Result.Succeeded)
                {
                    // Do other things
                }
            }
        }

        private void SeedUsers()
        {
            var users = new List<ApplicationUser>();
            // Add seed users to the list
            
            users.Add(new ApplicationUser
            {
                FirstName = "Generic",
                LastName = "Person",
                UserName = "person",
                Email = "person@domain.com",
                EmailConfirmed = true
            });

            foreach (var user in users)
            {
                var userTask = _userManager.CreateAsync(user, "Password1!");
                userTask.Wait();

                if (userTask.Result.Succeeded)
                {
                    var roles = new List<string>();
                    if (user.Email.Contains("@domain.com"))
                        roles = new List<string> { "Admin", "Developer" };
                    
                    var roleTask = _userManager.AddToRolesAsync(user, roles);
                    roleTask.Wait();

                    if (roleTask.Result.Succeeded)
                    {
                        // Do other things
                    }
                }
            }
        }
    }
}
