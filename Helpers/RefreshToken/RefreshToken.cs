using System;

namespace ColibriWebApi.Helpers.RefreshToken
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime ExpiredOn { get; set; }
    }
}
