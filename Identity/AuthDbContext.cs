using ColibriWebApi.DL;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ColibriWebApi.Identity
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
            optionsBuilder.UseSqlServer(_config["HomeDev:ConnectionStrings:AuthDbConnection"]);
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<FeedBack> Feedback { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
    }
}
