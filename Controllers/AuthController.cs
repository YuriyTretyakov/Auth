using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Authorization.ExternalLoginProvider;
using Authorization.ExternalLoginProvider.FaceBook;
using Authorization.ExternalLoginProvider.Google;
using Authorization.Identity;
using Authorization.ViewModels;
using Authorization.ViewModels.Auth;
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
        private readonly RefreshTokenStorage _refreshTokenStorage;


        public AuthController(SignInManager<User> signInManager,
            UserManager<User> userManager,
            IConfiguration configuration,
            FacebookLoginProvider faceBookProvider,
            GoogleLoginProvider googleProvider,
            RefreshTokenStorage refreshTokenStorage
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _faceBookProvider = faceBookProvider;
            _googleProvider = googleProvider;
            _refreshTokenStorage = refreshTokenStorage;
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

                    return Ok(new
                    {
                        Token = GenerateJwtToken(user),
                        RefreshToken = GenerateRefreshToken()
                    });
                }
            }

            return BadRequest();
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

            var token = await _googleProvider.GetToken(code);
            var userInfo = await _googleProvider.GetUserProfile(token.AccessToken);

            var identityUser = await ProcessExternalUser(userInfo, "Google");

            return Ok(new
            {
                Token = GenerateJwtToken(identityUser),
                RefreshToken = GenerateRefreshToken()
            });
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

            await _faceBookProvider.RequestToken(code);

            if (_faceBookProvider.Token == null)
                return BadRequest("Unable to retrieve access token");

            var userData = await _faceBookProvider.GetFacebookUserInfo();

            if (userData?.Email == null)
                return BadRequest("Unable to retrieve User's email which is required");

            var identityUser = await ProcessExternalUser(userData, "FaceBook");

            return Ok(new
            {
                Token = GenerateJwtToken(identityUser),
                RefreshToken = GenerateRefreshToken()
            });
        }

        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(string token, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var username = principal.Identity.Name;

            var isValidRefreshToken = _refreshTokenStorage.IsValidRefreshToken(username, refreshToken);

            if (!isValidRefreshToken)
                throw new SecurityTokenException("Invalid refresh token");

            var user = await _userManager.FindByEmailAsync(username);

            var newJwtToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            _refreshTokenStorage.RemoveRefreshToken(username, refreshToken);
            _refreshTokenStorage.AddRefreshToken(username, newRefreshToken);
            return Ok(new { token = newJwtToken, refreshToken = newRefreshToken });
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
                Audience = _configuration["JwtIssuer"],
                SigningCredentials = creds,
                Expires = expires,
                Subject = identity
            });

            return tokenHandler.WriteToken(token);
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

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("the server key used to sign the JWT token is here, use more than 16 chars")),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }


    }
}