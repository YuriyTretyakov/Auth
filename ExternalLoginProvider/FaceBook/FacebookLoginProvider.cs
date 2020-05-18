using System.Net.Http;
using System.Threading.Tasks;
using Authorization.ExternalLoginProvider.FaceBook.ResponseModels;
using Newtonsoft.Json;

namespace Authorization.ExternalLoginProvider.FaceBook
{
    public class FacebookLoginProvider
    {

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

        public async Task<UserProfile> GetFacebookUserInfo(string token)
        {
            if (token == null)
                return null;

            var url = $"https://graph.facebook.com/me?fields=id,first_name,last_name,middle_name,name,name_format,picture,short_name,email&access_token={token}";
            var data = await GetData<UserProfile>(url);
            return data;
        }
    }
}
