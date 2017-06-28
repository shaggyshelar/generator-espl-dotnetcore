using AppConfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Data
{
    // TODO: * This is a hack workaround for an EF bug when creating a new migration!
    //         You can't create a new migration with this junk because of the following error:
    //              No parameterless constructor was found on 'ApplicationDbContext'. Either add a 
    //              parameterless constructor to 'ApplicationDbContext' or add an implementation 
    //              of 'IDbContextFactory<ApplicationDbContext>' in the same assembly as 'ApplicationDbContext'.
    //       * Adding a parameterless constructor doesn't work either.
    //       * Reference: http://benjii.me/2016/05/dotnet-ef-migrations-for-asp-net-core/
    public class ApplicationDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContextFactory()
        {
        }

        public ApplicationDbContext Create(DbContextFactoryOptions options)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            builder.UseSqlServer(OptionsStore.ApplicationOptions.DbConnection);

            return new ApplicationDbContext(builder.Options);
        }
    }
}
