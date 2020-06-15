using System;
using System.Net.Http;
using System.Threading.Tasks;
using ColibriWebApi.ExternalLoginProvider.FaceBook;
using ColibriWebApi.ExternalLoginProvider.Google.ResponseModels;
using Newtonsoft.Json;

namespace ColibriWebApi.ExternalLoginProvider.Google
{
    public class GoogleLoginProvider
    {
        public async Task<UserProfile> GetUserProfile(string token)
        {
            if (token == null)
                return null;

            var url = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json";
            var data = await new DataProvider(token).GetData<UserProfile>(url);
            return data;
        }
    }
}
