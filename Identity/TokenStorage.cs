using System.Collections.Generic;

namespace Authorization.Identity
{
    public class TokenStorage
    {
        private readonly Dictionary<string, List<string>> _container =new Dictionary<string, List<string>>();

        public void AddToken(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) ||
                string.IsNullOrEmpty(token)) return;

            if (!_container.ContainsKey(userId))
                _container[userId] = new List<string> {token};
            else
                _container[userId].Add(token);
        }

        public bool IsValidToken(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) ||
                string.IsNullOrEmpty(token)) return false;

            if (!_container.ContainsKey(userId)) return false;

            return _container[userId].Contains(token);
        }

        public void RemoveToken(string userId, string token)
        {
            if  (string.IsNullOrEmpty(userId)||
             string.IsNullOrEmpty(token))return;

            if (_container.ContainsKey(userId))
                _container[userId].Remove(token);
        }

    }
}
