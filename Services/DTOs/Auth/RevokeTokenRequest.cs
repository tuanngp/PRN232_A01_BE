using System.ComponentModel.DataAnnotations;

namespace Services.DTOs.Auth
{
    public class RevokeTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
