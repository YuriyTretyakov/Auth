using System;
using System.Net.Http;
using System.Threading.Tasks;
using Authorization.ExternalLoginProvider.Google.ResponseModels;
using Newtonsoft.Json;

namespace Authorization.ExternalLoginProvider.Google
{
    public class GoogleLoginProvider
    {
        public  string ClientId { get; }
        public string ClientSecret { get; }
        public  string RedirectUrl { get; set; }

        private GoogleLoginProvider()
        {}

        public GoogleLoginProvider(string clientid, string clientsecret)
        {
            ClientId = clientid;
            ClientSecret = clientsecret;
        }

        public string GetLoginUrl()
        {
                var googleLoginUrl =  $"https://accounts.google.com/o/oauth2/auth?" +
                                                         $"redirect_uri={RedirectUrl}" +
                                                         $"&response_type=code&client_id={ClientId}" +
                                                         $"&scope=https://www.googleapis.com/auth/userinfo.email+https://www.googleapis.com/auth/userinfo.profile" +
                                                         $"&approval_prompt=force&access_type=offline";
                return googleLoginUrl;
        }

        public async Task<Token> GetToken(string code)
        {
            var accessReqBody = new RequestToken
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                Code = code,
                RedirectUri = RedirectUrl,
                GrantType = "authorization_code"
            };

            HttpResponseMessage tokenResponse;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://oauth2.googleapis.com");
                tokenResponse = await client.PostAsJsonAsync("token", accessReqBody);
            }

            if (!tokenResponse.IsSuccessStatusCode)
                return null;

            var tokenRespString = await tokenResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Token>(tokenRespString);
        }
        public async Task<UserProfile> GetUserProfile(string token)
        {
            HttpResponseMessage userInfoResponse;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/oauth2/v1/");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}");
                userInfoResponse = await client.GetAsync("userinfo?alt=json");
            }

            if (!userInfoResponse.IsSuccessStatusCode)
                return null;

            var userInfoStr = await userInfoResponse.Content.ReadAsStringAsync();
            var userInfo = JsonConvert.DeserializeObject<UserProfile>(userInfoStr);
            return userInfo;
        }
    }
}
