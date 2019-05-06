using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Authorization.Identity
{
    public class AuthDataSeeder
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private const string SeedUserName = "rootuser@authenticationservice.com";

        public AuthDataSeeder(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedUsers()
        {
            if (!await _roleManager.RoleExistsAsync("Administrator"))
                await _roleManager.CreateAsync(new IdentityRole { Name = "Administrator" });

            if (await _userManager.FindByEmailAsync(SeedUserName) == null)
            {
                var user = new User
                {
                    UserName = SeedUserName,
                    Email = SeedUserName
                };

                IdentityResult result = await _userManager.CreateAsync(user, "P@ssword");

                if (result.Succeeded)
                    await _userManager.AddToRolesAsync(user, new[] { "Administrator" });
            }
        }
    }
}
