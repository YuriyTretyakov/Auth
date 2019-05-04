using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Authorization.Identity
{
    public class AuthDbContext:IdentityDbContext<User>
    {
        private readonly IConfiguration _config;


        public AuthDbContext(IConfiguration config,DbContextOptions<AuthDbContext> options)
        {
            _config = config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_config["ConnectionStrings:AuthDbConnection"]);
        }
    }
}
