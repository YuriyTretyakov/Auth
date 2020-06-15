using System.Net.Http;
using System.Threading.Tasks;
using ColibriWebApi.ExternalLoginProvider.FaceBook.ResponseModels;
using Newtonsoft.Json;

namespace ColibriWebApi.ExternalLoginProvider.FaceBook
{
    public class FacebookLoginProvider
    {
        public async Task<IGenericUserExternalData> GetUserProfile(string token)
        {
            if (token == null)
                return null;

            var url = $"https://graph.facebook.com/me?fields=id,first_name,last_name,middle_name,name,name_format,picture,short_name,email&access_token={token}";
            var data = await new DataProvider().GetData<UserProfile>(url);
            return data;
        }
    }
}
