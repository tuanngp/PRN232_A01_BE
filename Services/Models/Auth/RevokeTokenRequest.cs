using System.ComponentModel.DataAnnotations;

namespace Services.Models.Auth
{
    public class RevokeTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
