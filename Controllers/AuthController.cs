using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Authorization.ExternalLoginProvider;
using Authorization.ExternalLoginProvider.FaceBook;
using Authorization.ExternalLoginProvider.Google;
using Authorization.ExternalLoginProvider.Google.ResponseModels;
using Authorization.Identity;
using Authorization.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

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


        //private readonly string _jwtKey = "SOME_RANDOM_KEY_DO_NOT_SHARE";
        //private readonly string _jwtIssuer = "http://yourdomain.com";
        //private readonly int _jwtExpireDays = 30;


        public AuthController(SignInManager<User> signInManager,
                              UserManager<User> userManager,
                              IConfiguration configuration,
                              FacebookLoginProvider faceBookProvider,
                              GoogleLoginProvider googleProvider
                              )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _faceBookProvider = faceBookProvider;
            _googleProvider = googleProvider;
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
                    return GenerateJwtToken(user);
                }

            }

            return BadRequest();
        }

        [AllowAnonymous]
        [HttpGet("LoginWithGoogle")]
        public void LoginWithGoogle()
        {
            _googleProvider.RedirectUrl= $"{Request.Scheme}://{Request.Host}/auth/GoogleCallBack/";
            var redirectUrl = _googleProvider.GetLoginUrl();


            //var redirectUrl = $"https://accounts.google.com/o/oauth2/auth?" +
            //                  $"redirect_uri=https://localhost:5001/Auth/GoogleCallBack/" +
            //                  $"&response_type=code&client_id={_configuration["ClientId"]}" +
            //                  $"&scope=https://www.googleapis.com/auth/userinfo.email+https://www.googleapis.com/auth/userinfo.profile" +
            //                  $"&approval_prompt=force&access_type=offline";

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

            return Ok(userInfo);

            //var accessReqBody = new RequestToken
            //{
            //    ClientId = _configuration["ClientId"],
            //    ClientSecret = _configuration["ClientSecret"],
            //    Code = code,
            //    RedirectUri = "https://localhost:5001/Auth/GoogleCallBack/",
            //    GrantType = "authorization_code"
            //};

            //HttpResponseMessage tokenResponse;

            //using (var client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri("https://oauth2.googleapis.com");
            //    tokenResponse = await client.PostAsJsonAsync("token", accessReqBody);
            //}

            //var tokenRespString = await tokenResponse.Content.ReadAsStringAsync();
            //var token = JsonConvert.DeserializeObject<Token>(tokenRespString);

            //if (token == null)
            //    return BadRequest("Unable to retrieve access token");

            //HttpResponseMessage userInfoResponse;

            //using (var client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri("https://www.googleapis.com/oauth2/v1/");
            //    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token.AccessToken}");
            //    userInfoResponse= await client.GetAsync("userinfo?alt=json");
            //}

            //var userInfoStr = await userInfoResponse.Content.ReadAsStringAsync();

            //var userInfo= JsonConvert.DeserializeObject<UserProfile>(userInfoStr);

            return Ok(userInfo);
        }

        [AllowAnonymous]
        [HttpGet("SignInFacebook")]
        public void LoginFacebook()
        {
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

            var user = await ProcessExternalUser(userData,"FaceBook");
            return Ok(user);
        }

        private async Task<User> ProcessExternalUser(IGenericUserExternalData userProfile,string externalProvider)
        {
            if (userProfile.Email == null) return null;
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
                    ExternalProviderId=userProfile.ExternalProviderId
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