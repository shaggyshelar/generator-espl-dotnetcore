using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApi.Models.AccountViewModels
{
    public class RegisterViewModel
    {

        [Display(Name = "Username")]
        public string Username {get; set;}

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Roles")]
        public IEnumerable<string> Roles {get; set;}
    }
}
