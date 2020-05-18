using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization.ViewModels.Auth.Request
{
    public class SocialUser
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Provider { get; set; }
        public string Provideid { get; set; }
        public string Image { get; set; }
        [Required(AllowEmptyStrings =false,ErrorMessage ="Token should be provided for social user")]
        public string AuthToken { get; set; }
        public string IdToken { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Id should be provided for social user")]
        public string Id { get; set; }
    }
}

