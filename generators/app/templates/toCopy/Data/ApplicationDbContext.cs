using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Interfaces;
using Models.Domain;

namespace Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        private bool _didMigrate = false;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            // This will always do an update-database when the app runs,
            // in case there are new migrations
            if (!_didMigrate)
            {
                Database.Migrate();
                _didMigrate = true;
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply stuff that isn't created from the default conventions
        }

        #region Domain models

        // Root level models
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }
        
        // Add other models here
        
        #endregion
    }
}
