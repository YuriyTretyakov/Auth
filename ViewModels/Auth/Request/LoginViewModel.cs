using System.ComponentModel.DataAnnotations;

namespace ColibriWebApi.ViewModels.Auth.Request
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
