using Microsoft.AspNetCore.Identity;
using System;

namespace Authorization.Identity
{
    public class User:IdentityUser
    {
        public string Name { get; set; }
        public DateTime RegisteredOn { get; set; }
        public DateTime LastLoggedInOn { get; set; }
    }
}
