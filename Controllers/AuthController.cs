using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Authorization.ExternalLoginProvider;
using Authorization.Identity;
using Authorization.ResponseModels;
using Authorization.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Authorization.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private FacebookLoginProvider _faceBookProvider;

       

        //private readonly string _jwtKey = "SOME_RANDOM_KEY_DO_NOT_SHARE";
        //private readonly string _jwtIssuer = "http://yourdomain.com";
        //private readonly int _jwtExpireDays = 30;


        public AuthController(SignInManager<User> signInManager,
                              UserManager<User> userManager,
                              IConfiguration configuration,
                              FacebookLoginProvider faceBookProvider)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _faceBookProvider = faceBookProvider;
        }

        [HttpPost("Login")]
        public async Task<object> Login([FromBody] LoginViewModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var signInResult =
                    await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, true, false);

                if (signInResult.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(loginModel.Email);
                    return GenerateJwtToken(loginModel.Email, user);
                }

            }

            return BadRequest();
        }

        [HttpGet("LoginWithGoogle")]
        public void LoginWithGoogle()
        {
            var redirectUrl = $"https://accounts.google.com/o/oauth2/auth?" +
                              $"redirect_uri=https://localhost:5001/signin-google&" +
                              $"response_type=code&client_id={_configuration["ClientId"]}&" +
                              $"scope=https://www.googleapis.com/auth/userinfo.email+https://www.googleapis.com/auth/userinfo.profile&" +
                              $"approval_prompt=force&access_type=offline";

            Response.Redirect(redirectUrl);
        }


        [AllowAnonymous]
        [HttpGet("SignInFacebook")]
        public void LoginFacebook()
        {
            _faceBookProvider.Secret = _configuration["FaceBookSecret"];
            _faceBookProvider.Appid = _configuration["FaceBookAppId"];
            _faceBookProvider.RedirectUrl= $"{Request.Scheme}://{Request.Host}/auth/FBCallback/";
            var loginfaceBookUrl = _faceBookProvider.GetLoginUrl();
            Response.Redirect(loginfaceBookUrl);
        }

        [AllowAnonymous]
        [HttpGet("FBCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string error_code = null, string error_message = null, string code = null)
        {
            if (error_code != null)
                return BadRequest(error_message);

            if (code == null)
                return BadRequest("Unable to retrieve verification code");

            await _faceBookProvider.RequestToken(code);

            if (_faceBookProvider.Token == null)
                return BadRequest("Unable to retrieve access token");

            var userData = await _faceBookProvider.GetFacebookUserInfo();

            if (userData?.Email == null)
                return BadRequest("Unable to retrieve User's email which is required");

            var user = await ProcessFacebookUser(userData);
            return Ok(user);
        }

        private async Task<User> ProcessFacebookUser(UserProfile userProfile)
        {
            if (userProfile.Email == null) return null;
            var user = await _userManager.FindByEmailAsync(userProfile.Email);

            if (user == null)
            {
                user = new User
                {
                    UserName = userProfile.Email,
                    Email = userProfile.Email,
                    Name = userProfile.FirstName,
                    LastName = userProfile.LastName,
                    EmailConfirmed = true,
                    UserPicture = userProfile.Picture.Data.Url,
                    RegisteredOn = DateTime.Now,
                    ExternalProvider = "Facebook",
                    ExternalProviderId=userProfile.Id.ToString()
                };
                await _userManager.CreateAsync(user);
            }
            await _signInManager.SignInAsync(user, isPersistent: false);
            user.LastLoggedInOn = DateTime.Now;
            return user;
        }

        private string GenerateJwtToken(string email, IdentityUser user)
        {
            var identity = new ClaimsIdentity(

                new System.Security.Principal.GenericIdentity(user.Email, "Token"),
                new[] { new Claim("ID", user.Id.ToString()) }
            );

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(int.Parse(_configuration["TokenLifeTimeMin"]));

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _configuration["JwtIssuer"],
                Audience = _configuration["JwtIssuer"],
                SigningCredentials = creds,
                Expires = expires,
                Subject = identity
            });

            return tokenHandler.WriteToken(token);
        }

    }
}