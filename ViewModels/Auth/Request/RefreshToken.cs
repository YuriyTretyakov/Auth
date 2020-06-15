using System.ComponentModel.DataAnnotations;

namespace ColibriWebApi.ViewModels.Auth.Request
{
    public class RefreshTokenRequest
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
