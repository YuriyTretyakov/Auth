using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization.Helpers.RefreshToken
{
    public class TokenValidationException:ApplicationException
    {
        public TokenValidationException(string message):base(message)
        {

        }
    }
}
