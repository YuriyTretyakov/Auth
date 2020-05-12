using System.ComponentModel.DataAnnotations;

namespace Authorization.ViewModels.Auth.Request
{
    public class CreateUserViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string Name { get; set; }

    }
}
