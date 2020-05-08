using System.Collections.Generic;

namespace Authorization.Identity
{
    public class TokenStorage
    {
        private readonly Dictionary<string, List<string>> _container =new Dictionary<string, List<string>>();

        public void AddToken(string user, string token)
        {
            if (string.IsNullOrEmpty(user) ||
                string.IsNullOrEmpty(token)) return;

            if (!_container.ContainsKey(user))
                _container[user] = new List<string> {token};
            else
                _container[user].Add(token);
        }

        public bool IsValidToken(string user, string token)
        {
            if (string.IsNullOrEmpty(user) ||
                string.IsNullOrEmpty(token)) return false;

            if (!_container.ContainsKey(user)) return false;

            return _container[user].Contains(token);
        }

        public void RemoveToken(string user, string token)
        {
            if  (string.IsNullOrEmpty(user)||
             string.IsNullOrEmpty(token))return;

            if (_container.ContainsKey(user))
                _container[user].Remove(token);
        }

    }
}
