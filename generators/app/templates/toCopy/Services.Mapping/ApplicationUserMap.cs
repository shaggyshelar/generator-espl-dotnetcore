using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Interfaces.Services;
using Models.Domain;
using Models.Dto;

namespace Services.Mapping
{
    public class ApplicationUserMap : BaseMap, IApplicationUserMap
    {
        public ApplicationUserMap(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public ApplicationUserSimpleDto ConvertToSimple(ApplicationUser applicationUser)
        {
            // We're going to turn this into a list so we can use the other method to convert it
            // and not duplicate the mapping
            return ConvertToSimple(new List<ApplicationUser> { applicationUser })
                .FirstOrDefault();
        }

        public IEnumerable<ApplicationUserSimpleDto> ConvertToSimple(IEnumerable<ApplicationUser> applicationUsers)
        {
            var users = from user in applicationUsers
                        select new ApplicationUserSimpleDto
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            UserName = user.UserName
                        };

            return users;
        }
    }
}
