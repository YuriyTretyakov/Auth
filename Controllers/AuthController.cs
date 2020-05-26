using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Authorization.ExternalLoginProvider;
using Authorization.ExternalLoginProvider.FaceBook;
using Authorization.ExternalLoginProvider.Google;
using Authorization.Helpers.RefreshToken;
using Authorization.ViewModels.Auth.Request;
using Authorization.ViewModels.Auth.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using User = Authorization.Identity.User;

namespace Authorization.Controllers
{

    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly FacebookLoginProvider _faceBookProvider;
        private readonly GoogleLoginProvider _googleProvider;
        private readonly TokenStorage _tokenStorage;


        public AuthController(SignInManager<User> signInManager,
            UserManager<User> userManager,
            IConfiguration configuration,
            TokenStorage tokenStorage
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _tokenStorage = tokenStorage;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var signInResult =
                    await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, true, false);

                if (signInResult.Succeeded)
                {
                    var identityUser = await _userManager.FindByEmailAsync(loginModel.Email);
                    var tokenContainer = GetTokenContainer(identityUser);
                    return Ok(tokenContainer);
                }
            }

            return BadRequest("Invalid user data provided");
        }


        

        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenContainer), 200)]
        [ProducesResponseType(typeof(BadRequestResult), 400)]
        [HttpPost("AddSocialUser")]
        public async Task<IActionResult> AddSocialUser(string token,SocialProviders provider)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Social network valid user token should be provided");

            IGenericUserExternalData userData=null;


            if (provider == SocialProviders.FaceBook)
                userData = await new FacebookLoginProvider().GetUserProfile(token);
            else if (provider == SocialProviders.Google)
                userData = await new GoogleLoginProvider().GetUserProfile(token);
            else
                return BadRequest($"Unknown LoginProvider {provider}");



            if (userData?.Email == null)
                return BadRequest($"Unable to retrieve User's email from {provider} profile" +
                                  $"Please go to your {provider} profile and make sure you have added email " +
                                  $"in case you want to login with your {provider} user");

            var identityUser = await ProcessExternalUser(userData, provider.ToString());

            var tokenContainer = GetTokenContainer(identityUser);
            return Ok(tokenContainer);
        }


        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Refresh token or user id");
            try
            {
                _tokenStorage.ValidateToken(refreshTokenRequest.UserId, refreshTokenRequest.RefreshToken);
                var user = await _userManager.FindByIdAsync(refreshTokenRequest.UserId);
                var tokenContainer = GetTokenContainer(user, refreshTokenRequest.RefreshToken);
                return Ok(tokenContainer);
            }
            catch (TokenValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }   

        private async Task<User> ProcessExternalUser(IGenericUserExternalData userProfile, string externalProvider)
        {
            var user = await _userManager.FindByEmailAsync(userProfile.Email);

            if (user == null)
            {
                user = new User
                {
                    UserName = userProfile.Email,
                    Email = userProfile.Email,
                    Name = userProfile.Name,
                    LastName = userProfile.LastName,
                    EmailConfirmed = true,
                    UserPicture = userProfile.UserPicture,
                    RegisteredOn = DateTime.Now,
                    ExternalProvider = externalProvider,
                    ExternalProviderId = userProfile.ExternalProviderId
                };
                await _userManager.CreateAsync(user);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            user.LastLoggedInOn = DateTime.Now;
            return user;
        }

        private string GenerateJwtToken(User user)
        {
            Claim[] claims = new[] { new Claim("ID", user.Id.ToString()),
                new Claim("Name", $"{user.Name} {user.LastName}"),
                new Claim("Pict",user.UserPicture)
            };

            var identity = new ClaimsIdentity(
                new System.Security.Principal.GenericIdentity(user.Email, "Token"),
                claims);


            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(int.Parse(_configuration["TokenLifeTimeMin"]));

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _configuration["JwtIssuer"],
                Audience = _configuration["JwtAudince"],
                SigningCredentials = creds,
                Expires = expires,
                Subject = identity
            });

            return tokenHandler.WriteToken(token);
        }

        private TokenContainer GetTokenContainer(User user,string oldRefreshToken=null)
        {
            var refreshToken = new RefreshTokenGenerator().Generate(_configuration.GetValue<TimeSpan>("RefreshTokenLifetime"));

            if (oldRefreshToken != null)
                _tokenStorage.RemoveToken(user.Id, oldRefreshToken);
            _tokenStorage.AddToken(user.Id, refreshToken);

            var newJwtToken = GenerateJwtToken(user);

            return new TokenContainer
            {
                Id=user.Id,
                Token=newJwtToken,
                RefreshToken=refreshToken.Token
            };
        }

        
    }
}