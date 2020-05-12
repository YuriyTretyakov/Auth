using System.ComponentModel.DataAnnotations;

namespace Authorization.ViewModels.Auth.Request
{
    public class RefreshTokenRequest
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
