using Interfaces.Seeds;
using Interfaces.Services;

namespace Services
{
    public class SeedService : ISeedService
    {

        private readonly IApplicationUserSeed _applicationUserSeed;

        public SeedService(IApplicationUserSeed applicationUserSeed)
        {
            _applicationUserSeed = applicationUserSeed;
        }

        public void Run()
        {
            _applicationUserSeed.Process();
        }
    }
}
