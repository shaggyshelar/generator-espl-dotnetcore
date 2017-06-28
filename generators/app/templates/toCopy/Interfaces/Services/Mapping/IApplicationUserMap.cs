using System.Collections.Generic;
using Models.Domain;
using Models.Dto;

namespace Interfaces.Services
{
    public interface IApplicationUserMap
    {
        ApplicationUserSimpleDto ConvertToSimple(ApplicationUser applicationUser);
        IEnumerable<ApplicationUserSimpleDto> ConvertToSimple(IEnumerable<ApplicationUser> applicationUsers);
    }
}
