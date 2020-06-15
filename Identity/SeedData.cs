using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColibriWebApi.Identity
{
    public class AuthDataSeeder
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private const string _seedUserName = "rootuser@authenticationservice.com";

        public AuthDataSeeder(AuthDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedUsers()
        {
            if (!await _roleManager.RoleExistsAsync("Administrator"))
                await _roleManager.CreateAsync(new IdentityRole { Name = "Administrator" });

            if (await _userManager.FindByEmailAsync(_seedUserName) == null)
            {
                var user = new User
                {
                    UserName = _seedUserName,
                    Email = _seedUserName,
                    Name="Administrator",
                    LastName="Root",
                    UserPicture= "https://itpotok.ru/wp-content/uploads/2014/08/image_1-251x300.png"
                };

                IdentityResult result = await _userManager.CreateAsync(user, "P@ssword");

                if (result.Succeeded)
                    await _userManager.AddToRolesAsync(user, new[] { "Administrator"});
            }
        }
    }
}
