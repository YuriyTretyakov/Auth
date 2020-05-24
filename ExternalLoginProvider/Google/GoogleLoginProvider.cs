using System;
using System.Net.Http;
using System.Threading.Tasks;
using Authorization.ExternalLoginProvider.FaceBook;
using Authorization.ExternalLoginProvider.Google.ResponseModels;
using Newtonsoft.Json;

namespace Authorization.ExternalLoginProvider.Google
{
    public class GoogleLoginProvider
    {
        public async Task<UserProfile> GetUserProfile(string token)
        {
            if (token == null)
                return null;

            var url = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json";
            var data = await new DataProvider(token).GetUserData<UserProfile>(url);
            return data;
        }
    }
}
