using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }
    }
}
