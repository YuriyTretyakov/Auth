using Newtonsoft.Json;

namespace Authorization.ExternalLoginProvider.FaceBook.ResponseModels
{
    public class Data
    {
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("is_silhouette")]
        public bool IsSilhouette { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
    }

    public class Picture
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class UserProfile:IGenericUserExternalData, ICanContainError
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("picture")]
        public Picture Picture { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }

        public string UserName => Email;
        public string Name => FirstName;
        public string UserPicture => Picture?.Data.Url;
        public string ExternalProviderId => Id;

        public bool IsError { get; set; }
        public string Message { get; set; }
    }

    public interface ICanContainError
    {
        bool IsError { get; set; }
        string Message { get; set; }
    }

}

