using System;
using System.Security.Cryptography;

namespace ColibriWebApi.Helpers.RefreshToken
{
    public class RefreshTokenGenerator
    {
        public RefreshToken Generate(TimeSpan tokenLifetime)
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);

                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomNumber),
                    ExpiredOn = DateTime.UtcNow + tokenLifetime
                };
            }
        }
    }
}
