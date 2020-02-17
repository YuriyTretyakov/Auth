using Microsoft.AspNetCore.Identity;
using System;

namespace Authorization.Identity
{
    public class User:IdentityUser
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public DateTime RegisteredOn { get; set; }
        public DateTime LastLoggedInOn { get; set; }
        public string UserPicture { get; set; }
        public string ExternalProvider { get; set; }
        public int? ExternalProviderId { get; set; }
    }
}
