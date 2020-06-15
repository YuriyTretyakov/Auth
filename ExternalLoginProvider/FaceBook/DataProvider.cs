using ColibriWebApi.ExternalLoginProvider.FaceBook.ResponseModels;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace ColibriWebApi.ExternalLoginProvider
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

        public async Task<TData> GetData<TData>(string url) where TData : ICanContainError,new()
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
            {
                var result = new TData
                {
                    IsError = true,
                    Message = await response.Content.ReadAsStringAsync()
                };
                return result;
            }
             
            return JsonConvert.DeserializeObject<TData>(jsonStr);
        }

        public async Task<TData> PostData<TData>(string url,object obj) where TData : ICanContainError, new()
        {
            HttpResponseMessage response;

            using (var client = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(_bearerToken))
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_bearerToken}");

                var json= JsonConvert.SerializeObject(obj);
                response = await client.PostAsync(url,new StringContent(json));
            }
            var jsonStr = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var result = new TData
                {
                    IsError = true,
                    Message = await response.Content.ReadAsStringAsync()
                };
                return result;
            }

            return JsonConvert.DeserializeObject<TData>(jsonStr);
        }
    }
}
