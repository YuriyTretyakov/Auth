using Newtonsoft.Json;

namespace Authorization.ExternalLoginProvider.Google.ResponseModels
{
    public class UserProfile:IGenericUserExternalData
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("verified_email")]
        public bool VerifiedEmail { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("given_name")]
        public string GivenName { get; set; }
        [JsonProperty("family_name")]
        public string FamilyName { get; set; }
        [JsonProperty("picture")]
        public string Picture { get; set; }
        [JsonProperty("locale")]
        public string Locale { get; set; }

        public string LastName => FamilyName;
        public string UserPicture => Picture;
        public string ExternalProviderId => Id;
        public string UserName => Email;
    }
}
