using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Authorization.ExternalLoginProvider.FaceBook
{
    public class DataProvider
    {
        private readonly string _bearerToken;

        public DataProvider()
        {
        }

        public DataProvider(string bearerToken)
        {
            _bearerToken = bearerToken;
        }

        public async Task<TData> GetUserData<TData>(string url) where TData : class
        {
            HttpResponseMessage response;

            using (var client = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(_bearerToken))
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_bearerToken}");

                response = await client.GetAsync(url);
            }
            var jsonStr = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            return JsonConvert.DeserializeObject<TData>(jsonStr);
        }
    }
}
