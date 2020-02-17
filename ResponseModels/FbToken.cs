using Newtonsoft.Json;

namespace Authorization.ResponseModels
{
    public class FbToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        //"token_type":"bearer","expires_in":5168570}
    }
}
