using Microsoft.EntityFrameworkCore;
using UserAppService.Models;

namespace UserAppService.Data
{
    public class UserServiceContext : DbContext
    {
        public UserServiceContext(DbContextOptions<UserServiceContext> options) : base(options)
        { }

        public DbSet<User> User { get; set; }
        public DbSet<IntegrationEvent> IntegrationEventOutbox { get; set; }
    }
}
