using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Authorization.Identity;
using Authorization.ViewModels;
using Microsoft.AspNetCore.Authorization;
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


        //private readonly string _jwtKey = "SOME_RANDOM_KEY_DO_NOT_SHARE";
        //private readonly string _jwtIssuer = "http://yourdomain.com";
        //private readonly int _jwtExpireDays = 30;


        public AuthController(SignInManager<User> signInManager, UserManager<User> userManager,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
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


      
        private IActionResult Externallogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("FBCallback", "Auth", new {ReturnUrl = returnUrl});
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

      
        [HttpGet("SignInFacebook")]
        public void LoginFacebook()
        {
            var loginfaceBookUrl =$"https://www.facebook.com/v6.0/dialog/oauth?" +
                                  $"client_id={_configuration["FaceBookAppId"]}" +
                                  $"&client_secret={_configuration["FaceBookSecret"]}" +
                                  $"&redirect_uri=https://localhost:5001/auth/FBCallback" +
                                  $"&scope=email";
            Response.Redirect(loginfaceBookUrl);
          //  Externallogin("Facebook", "FBCallback");
        }


        private string GenerateJwtToken(string email, IdentityUser user)
        {
            var identity = new ClaimsIdentity(

                new System.Security.Principal.GenericIdentity(user.Email, "Token"),
                new[] {new Claim("ID", user.Id.ToString())}
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

        [AllowAnonymous]
        [HttpGet("FBCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnurl = null, string remoteError = null)
        {
            returnurl = returnurl ?? Url.Content("~/");
         //   if (remoteError != null)
         //   {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                    info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

                if (signInResult.Succeeded)
                    return LocalRedirect(returnurl);

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                if (email != null)
                {
                    var user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new User
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };
                        await _userManager.CreateAsync(user);

                        await _userManager.AddLoginAsync(user, info);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnurl);

                    }

                }
          //  }

            return null;
        }
    }
}