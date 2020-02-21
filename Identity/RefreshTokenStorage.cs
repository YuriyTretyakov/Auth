using System.Collections.Generic;

namespace Authorization.Identity
{
    public class RefreshTokenStorage:Dictionary<string,List<string>>
    {
        public void AddRefreshToken(string user, string token)
        {
            if (string.IsNullOrEmpty(user) ||
                string.IsNullOrEmpty(token)) return;

            if (!this.ContainsKey(user))
                this[user] = new List<string> {token};
            else
                this[user].Add(token);
        }

        public bool IsValidRefreshToken(string user, string token)
        {
            if (string.IsNullOrEmpty(user) ||
                string.IsNullOrEmpty(token)) return false;

            if (!this.ContainsKey(user)) return false;

            return this[user].Contains(token);
        }

        public void RemoveRefreshToken(string user, string token)
        {
            if  (string.IsNullOrEmpty(user)||
             string.IsNullOrEmpty(token))return;

            if (this.ContainsKey(user))
                this[user].Remove(token);
        }

    }
}
