using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Authorization.ExternalLoginProvider;
using Authorization.ExternalLoginProvider.FaceBook;
using Authorization.ExternalLoginProvider.Google;
using Authorization.Identity;
using Authorization.ViewModels.Auth;
using Authorization.ViewModels.Auth.Request;
using Authorization.ViewModels.Auth.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
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
            FacebookLoginProvider faceBookProvider,
            GoogleLoginProvider googleProvider,
            TokenStorage tokenStorage
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _faceBookProvider = faceBookProvider;
            _googleProvider = googleProvider;
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
        [HttpGet("LoginWithGoogle")]
        public void LoginWithGoogle()
        {
            _googleProvider.RedirectUrl = $"{Request.Scheme}://{Request.Host}/auth/GoogleCallBack/";
            var redirectUrl = _googleProvider.GetLoginUrl();
            Response.Redirect(redirectUrl);
        }


        [AllowAnonymous]
        [HttpGet("GoogleCallBack")]
        public async Task<IActionResult> GoogleCallBack(string code = null)
        {
            if (code == null)
                return BadRequest("Unable to retrieve verification code");

            _googleProvider.RedirectUrl = $"{Request.Scheme}://{Request.Host}/auth/GoogleCallBack/";

            var token = await _googleProvider.GetToken(code);

            if (token==null)
                return BadRequest("Unable to retrieve user token by verification code");

            var userInfo = await _googleProvider.GetUserProfile(token.AccessToken);

            if (userInfo == null)
                return BadRequest("Unable to retrieve user info by token provided");

            var identityUser = await ProcessExternalUser(userInfo, "Google");
            var tokenContainer = GetTokenContainer(identityUser);
            return Ok(tokenContainer);

        }

        [AllowAnonymous]
        [HttpGet("SignInFacebook")]
        public void LoginFacebook()
        {
            _faceBookProvider.RedirectUrl = $"{Request.Scheme}://{Request.Host}/auth/FBCallback/";
            var loginfaceBookUrl = _faceBookProvider.GetLoginUrl();
            Response.Redirect(loginfaceBookUrl);
        }

        [AllowAnonymous]
        [HttpGet("FBCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string error_code = null, string error_message = null,
            string code = null)
        {
            if (error_code != null)
                return BadRequest(error_message);

            if (code == null)
                return BadRequest("Unable to retrieve verification code");

            _faceBookProvider.RedirectUrl = $"{Request.Scheme}://{Request.Host}/auth/FBCallback/";
            await _faceBookProvider.RequestToken(code);

            if (_faceBookProvider.Token == null)
                return BadRequest("Unable to retrieve access token");

            var userData = await _faceBookProvider.GetFacebookUserInfo();

            if (userData?.Email == null)
                return BadRequest("Unable to retrieve User's email which is required");

            var identityUser = await ProcessExternalUser(userData, "FaceBook");

            var tokenContainer = GetTokenContainer(identityUser);

            //var userToken = GenerateJwtToken(identityUser);
            //_tokenStorage.AddToken(identityUser.Email, userToken);
            return Ok(tokenContainer);
        }

        
        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            if (ModelState.IsValid)
            {
                var isValidToken = _tokenStorage.IsValidToken(refreshTokenRequest.UserId, refreshTokenRequest.RefreshToken);

                if (isValidToken)
                {
                    var user = await _userManager.FindByIdAsync(refreshTokenRequest.UserId);
                    var tokenContainer = GetTokenContainer(user);
                    return Ok(tokenContainer);
                }
            }
            return BadRequest("Invalid token or user id");
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

        private string GenerateJwtToken(IdentityUser user)
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
                Audience = _configuration["JwtAudince"],
                SigningCredentials = creds,
                Expires = expires,
                Subject = identity
            });

            return tokenHandler.WriteToken(token);
        }

        private TokenContainer GetTokenContainer(User user,string oldRefreshToken=null)
        {
            var newJwtToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            if (oldRefreshToken!=null)
                _tokenStorage.RemoveToken(user.Id, oldRefreshToken);
            _tokenStorage.AddToken(user.Id, refreshToken);

            return new TokenContainer
            {
                Id=user.Id,
                Token=newJwtToken,
                RefreshToken=refreshToken
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}