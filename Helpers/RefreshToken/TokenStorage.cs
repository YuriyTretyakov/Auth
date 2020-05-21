using System;
using System.Collections.Generic;
using System.Linq;

namespace Authorization.Helpers.RefreshToken
{
    

    public class TokenStorage
    {
        private readonly Dictionary<string, List<RefreshToken>> _container =new Dictionary<string, List<RefreshToken>>();

        public void AddToken(string userId, RefreshToken tokenData)
        {
            CleanExpiredTokens();

            if (string.IsNullOrEmpty(userId) ||
                tokenData?.Token == null ||
                tokenData.ExpiredOn == DateTime.MinValue)

                throw new ApplicationException($"Can't Add refresh token with such data: userId={userId} Token={tokenData.Token} ExpiredOn={tokenData.ExpiredOn}");

            if (!_container.ContainsKey(userId))
                _container[userId] = new List<RefreshToken> { tokenData };
            else
                _container[userId].Add(tokenData);
        }

        public void ValidateToken(string userId, string token)
        {
            

            if (string.IsNullOrEmpty(userId) ||
                string.IsNullOrWhiteSpace(token))
                throw new TokenValidationException("User id and token should be provided");

            if (!_container.ContainsKey(userId))
                throw new TokenValidationException($"No token was found for user with id: {userId}");

            CleanExpiredTokens();

            // var isValidToken = _container[userId].Any(t => t.Token.Equals(token)&&t.ExpiredOn>DateTime.UtcNow);

             var isValidToken = _container[userId].Any(t => t.Token.Equals(token));

            if (!isValidToken)
                throw new TokenValidationException($"Unknown refresh token for user:{userId}");
        }

        public void RemoveToken(string userId, string token)
        {
            if  (string.IsNullOrEmpty(userId)||
                string.IsNullOrWhiteSpace(token))
                return;

            if (_container.ContainsKey(userId))
            {
                var concreteItem = _container[userId].FirstOrDefault(t => t.Token== token);

                if (concreteItem!=null)
                _container[userId].Remove(concreteItem);
            }
        }

        public void CleanExpiredTokens()
        {
            foreach (var key in _container.Keys)
            {
                foreach(var val in _container[key].ToList())
                {
                    if (val.ExpiredOn < DateTime.UtcNow)
                        _container[key].Remove(val);
                }
            }
        }
    }
}
