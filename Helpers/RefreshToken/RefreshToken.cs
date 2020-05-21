using System;

namespace Authorization.Helpers.RefreshToken
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime ExpiredOn { get; set; }
    }
}
