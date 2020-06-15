using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColibriWebApi.Helpers.RefreshToken
{
    public class TokenValidationException:ApplicationException
    {
        public TokenValidationException(string message):base(message)
        {

        }
    }
}
