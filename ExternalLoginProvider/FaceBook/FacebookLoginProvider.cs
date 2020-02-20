using System.Net.Http;
using System.Threading.Tasks;
using Authorization.ExternalLoginProvider.FaceBook.ResponseModels;
using Newtonsoft.Json;

namespace Authorization.ExternalLoginProvider.FaceBook
{
    public class FacebookLoginProvider
    {

        public string AppScope { get; set; }= "email";
        public string UserRequestScope { get; set; } = "first_name,last_name,email,picture,birthday,gender";
        public readonly string DisplayOauthDialog = "iframe";

        public string Appid { get; private set; }
        public string Secret { get; private set; }
        public string RedirectUrl { get; set; }

        public FBToken Token { private set; get; }

        private FacebookLoginProvider()
        { }

        public FacebookLoginProvider(string appid, string appsecret)
        {
            Appid = appid;
            Secret = appsecret;
        }

        public async Task<FBToken> GetToken(string code)
        {
            var url = $"https://graph.facebook.com/oauth/access_token?" +
                $"client_id={Appid}" +
                $"&client_secret={Secret}" +
                $"&code={code}" +
                $"&redirect_uri={RedirectUrl}";

            var data = await GetData<FBToken>(url);
            return data;
        }

        public async Task RequestToken(string code)
        {
            Token = await GetToken(code);
        }


        private async Task<TData> GetData<TData>(string url) where TData : class
        {
            HttpResponseMessage response;

            using (var client = new HttpClient())
            {
                 response = await client.GetAsync(url);
            }
            var jsonStr = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            return JsonConvert.DeserializeObject<TData>(jsonStr);
        }

        public string GetLoginUrl()
        {
            var loginfaceBookUrl = $"https://www.facebook.com/v6.0/dialog/oauth?" +
                                  $"client_id={Appid}" +
                                  $"&redirect_uri={RedirectUrl}" +
                                  $"&scope={AppScope}" +
                                  $"&display={DisplayOauthDialog}";
            return loginfaceBookUrl;
        }

        public async Task<UserProfile> GetFacebookUserInfo()
        {
            if (Token == null)
                return null;

            var url = $"https://graph.facebook.com/me?fields={UserRequestScope}&access_token={Token.AccessToken}";
            var data = await GetData<UserProfile>(url);
            return data;
        }


    }
}
