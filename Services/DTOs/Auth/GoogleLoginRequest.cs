using System.ComponentModel.DataAnnotations;

namespace Services.DTOs.Auth
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
} 