using Data;
using Interfaces;

namespace Services
{
    public abstract class BaseMap
    {
        protected readonly ApplicationDbContext DbContext;

        public BaseMap(IApplicationDbContext dbContext)
        {
            DbContext = dbContext as ApplicationDbContext;
        }
    }
}
