using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

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
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            optionsBuilder.UseSqlServer(_config[$"{env}:ConnectionStrings:AuthDbConnection"]);
        }
    }
}
